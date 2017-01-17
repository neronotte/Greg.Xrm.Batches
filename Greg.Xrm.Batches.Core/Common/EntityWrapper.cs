using System;
using System.IO;
using System.Runtime.CompilerServices;
using Avanade;
using Avanade.Rina.Batches.Core.Common;
using Common.Logging;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace Greg.Xrm.Batches.Core.Common
{
	/// <summary>
	/// Base class to be used as wrapper for the CRM Entities
	/// </summary>
	public abstract class EntityWrapper
	{
		private readonly ILog log;
		private readonly Entity preImage;
		private readonly Entity target;


		protected EntityWrapper(Entity entity)
		{
			this.log = LogManager.GetLogger(this.GetType());
			this.preImage = entity;
			this.target = new Entity(entity.LogicalName) {Id = entity.Id};
			this.IsDeleted = false;
		}

		protected EntityWrapper(string entityName)
		{
			this.log = LogManager.GetLogger(this.GetType());
			this.preImage = new Entity(entityName);
			this.target = new Entity(entityName);
			this.IsDeleted = false;
		}

		/// <summary>
		/// The unique identifier of the current entity
		/// </summary>
		public Guid Id => this.target.Id;


		/// <summary>
		/// Indicates whether the current entity is a new entity or is already been created.
		/// </summary>
		[JsonIgnore]
		public bool IsNew => this.target.Id == Guid.Empty;

		/// <summary>
		/// Gets the logical name of the current entity
		/// </summary>
		[JsonIgnore]
		public string EntityName => this.target.LogicalName;

		/// <summary>
		/// Indicates whether the current entity has been deleted or not
		/// </summary>
		[JsonIgnore]
		public bool IsDeleted { get; private set; }



		/// <summary>
		/// Gets the value of the property with the given <paramref name="propertyName"/>.
		/// If not specified, takes the name of the Member (Property or Method) invoking the current function.
		/// </summary>
		/// <typeparam name="T">The type of the property value</typeparam>
		/// <param name="propertyName">The name of the property to retrieve</param>
		/// <returns>
		/// The value of the property with the given name
		/// </returns>
		protected T Get<T>([CallerMemberName]string propertyName = null)
		{
			// recupero il campo dal target
			if (this.target.Contains(propertyName))
			{
				return this.target.GetAttributeValue<T>(propertyName);
			}

			// poi dalla pre-image
			return this.preImage.GetAttributeValue<T>(propertyName);
		}

		/// <summary>
		/// Gets the value of the aliased property with the given <paramref name="propertyName"/>.
		/// <paramref name="propertyName"/> must include the entity alias.
		/// </summary>
		/// <typeparam name="T">The type of the property value</typeparam>
		/// <param name="propertyName">The name of the property to retrieve, including the alias</param>
		/// <returns>
		/// The value of the property with the given name
		/// </returns>
		protected T GetAliased<T>(string propertyName)
		{
			var aliasedValue = this.target.GetAttributeValue<AliasedValue>(propertyName) ??
			                   this.preImage.GetAttributeValue<AliasedValue>(propertyName);
			if (aliasedValue == null)
			{
				return default(T);
			}
			return (T)aliasedValue.Value;
		}

		/// <summary>
		/// Gets the value of the aliased property with the given <paramref name="propertyName"/>.
		/// <paramref name="propertyName"/> must not include the entity alias.
		/// </summary>
		/// <typeparam name="T">The type of the property value</typeparam>
		/// <param name="alias">The alias of the related entity containing the property value to be retrieved</param>
		/// <param name="propertyName">The name of the property to retrieve, without the alias</param>
		/// <returns>
		/// The value of the property with the given name
		/// </returns>
		protected T GetAliased<T>(string alias, string propertyName)
		{
			return this.GetAliased<T>($"{alias}.{propertyName}");
		}

		/// <summary>
		/// Sets the <paramref name="value"/> of the property with the given <paramref name="propertyName"/>.
		/// If not specified, takes the name of the Member (Property or Method) invoking the current function.
		/// </summary>
		/// <typeparam name="T">The type of the property value</typeparam>
		/// <param name="value">The value of the property</param>
		/// <param name="propertyName">The name of the property to set</param>
		protected void SetValue<T>(T value, [CallerMemberName] string propertyName = null)
		{
			this.Set(propertyName, value);
		}


		/// <summary>
		/// Sets the <paramref name="value"/> of the property with the given <paramref name="propertyName"/>.
		/// If not specified, takes the name of the Member (Property or Method) invoking the current function.
		/// </summary>
		/// <typeparam name="T">The type of the property value</typeparam>
		/// <param name="value">The value of the property</param>
		/// <param name="propertyName">The name of the property to set</param>
		protected void Set<T>(string propertyName, T value)
		{
			if (!this.preImage.Contains(propertyName))
			{
				if (value == null)
				{
					// do nothing;
					return;
				}

				this.target[propertyName] = value;
				return;
			}

			var preImageValue = this.preImage.GetAttributeValue<T>(propertyName);
			

			if (AreEqual(value, preImageValue))
			{
				this.target.Attributes.Remove(propertyName);
			}
			else
			{
				this.target[propertyName] = value;
			}
		}

		private static bool AreEqual<T>(T value, T preImageValue)
		{
			if (typeof(T) == typeof(EntityReference))
			{
				var a = value as EntityReference;
				var b = preImageValue as EntityReference;

				return Equals(a?.Id, b?.Id) && Equals(a?.LogicalName, b?.LogicalName);
			}
			if (typeof(T) == typeof(OptionSetValue))
			{
				var a = value as OptionSetValue;
				var b = preImageValue as OptionSetValue;

				return Equals(a?.Value, b?.Value);
			}
			if (typeof(T) == typeof(Money))
			{
				var a = value as Money;
				var b = preImageValue as Money;

				return Equals(a?.Value, b?.Value);
			}
			if (typeof (T) == typeof (string))
			{
				var a = value as string;
				var b = preImageValue as string;

				return string.Equals(a, b, StringComparison.Ordinal);
			}

			return Equals(value, preImageValue);
		}

		/// <summary>
		/// Saves or updates the current entity, committing the entity changes
		/// </summary>
		/// <param name="service">The organization service to be used to save or update the changes.</param>
		/// <returns>
		/// A value indicating whether the current entity has been correcly saved or updated.
		/// </returns>
		public virtual bool SaveOrUpdate(IOrganizationService service)
		{
			if (this.IsDeleted)
			{
				this.log.Error($"Entity {this.EntityName} ({this.Id}) is deleted! Save or Update failed");
				return false;
			}

			if (this.IsNew) // isNew
			{
				using(this.log.Track($"Creating {this.EntityName}: {this.ToJson()}"))
				{
					this.target.Id = service.Create(this.target);
					this.preImage.Id = this.target.Id;
				}
			}
			else if (this.target.Attributes.Count > 0)
			{
				using (this.log.Track($"Updating {this.EntityName}: {this.ToJson()}"))
				{
					service.Update(this.target);
				}
			}
			else
			{
				this.log.Debug($"No changes on {this.EntityName}: {this}");
			}
			


			// merge
			foreach (var key in this.target.Attributes.Keys)
			{
				this.preImage[key] = this.target[key];
			}
			this.target.Attributes.Clear();
			return true;
		}

		/// <summary>
		/// Deletes the current entity
		/// </summary>
		/// <param name="service">The organization service to be used to save or update the changes.</param>
		public void Delete(IOrganizationService service)
		{
			if (this.IsNew) 
				return;

			service.Delete(this.ToEntityReference());
			this.IsDeleted = true;
		}

		/// <summary>
		/// Provides a Json representation of the current object
		/// </summary>
		/// <returns>
		/// A json representation of the current object.
		/// </returns>
		public string ToJson()
		{
			var sw = new StringWriter();
			JsonSerializer.Create(new JsonSerializerSettings
			{
				Formatting = Formatting.Indented
			}).Serialize(sw, this);

			return sw.GetStringBuilder().ToString();
		}

		/// <summary>
		/// Returns an entity reference to the current object
		/// </summary>
		/// <returns>
		/// An entity reference to the current object
		/// </returns>
		public EntityReference ToEntityReference()
		{
			return this.preImage.ToEntityReference();
		}
	}
}