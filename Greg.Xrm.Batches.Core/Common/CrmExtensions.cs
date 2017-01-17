using Common.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Avanade
{
	public static class CrmExtensions
	{
		public static List<List<T>> Pool<T>(this IEnumerable<T> items, int poolSize)
		{
			var poolContainer = new List<List<T>>();

			var pool = new List<T>();
			poolContainer.Add(pool);
			
			foreach (var item in items)
			{
				pool.Add(item);

				if (pool.Count >= poolSize)
				{
					pool = new List<T>();
					poolContainer.Add(pool);
				}
			}

			return poolContainer;
		}



		public static void Delete(this IOrganizationService service, EntityReference entityRef)
		{
			if (service == null)
				throw new ArgumentNullException(nameof(service));
			if (entityRef == null)
				throw new ArgumentNullException(nameof(entityRef));

			service.Delete(entityRef.LogicalName, entityRef.Id);
		}


		public static List<Entity> RetrieveAll(this IOrganizationService service, QueryExpression query)
		{
			return RetrieveAll(service, query, x => x);
		}

		public static List<T> RetrieveAll<T>(this IOrganizationService service, QueryExpression query, Func<Entity, T> projection)
		{
			var log = LogManager.GetLogger<IOrganizationService>();
			log.DebugFormat("Fetching {0}...", query.EntityName);
			var sw = Stopwatch.StartNew();

			try
			{
				query.PageInfo.PageNumber = 1;
				query.PageInfo.Count = 5000;

				EntityCollection entityCollection;
				var result = new List<T>();
				do
				{
					log.DebugFormat("Fetching {0}...Query {1}", query.EntityName, query.PageInfo.PageNumber);

					entityCollection = service.RetrieveMultiple(query);
					if (entityCollection.MoreRecords)
					{
						query.PageInfo.PageNumber++;
						query.PageInfo.PagingCookie = entityCollection.PagingCookie;
					}

					result.AddRange(entityCollection.Entities.Select(projection));

				} while (entityCollection.MoreRecords);


				log.DebugFormat("Fetching {0}...Found a total of {1} records", query.EntityName, result.Count);

				return result;
			}
			finally
			{
				sw.Stop();
				log.DebugFormat("Fetching {0}...COMPLETED in: {1}", query.EntityName, sw.Elapsed);
			}


		}

		/// <summary>
		/// Retrieves a record.
		/// </summary>
		/// <param name="organizationService">The organization service used to retrieve the record.</param>
		/// <param name="entityReference">An entity reference pointing to the record to retrieve.</param>
		/// <param name="columns">The list of columns to retrieve</param>
		/// <returns>
		/// The retrieved entity.
		/// </returns>
		public static Entity Retrieve(this IOrganizationService organizationService, EntityReference entityReference, params string[] columns)
		{
			ColumnSet columnSet;
			if (columns == null || columns.Length == 0)
			{
				columnSet = new ColumnSet(false);
			}
			else
			{
				columnSet = new ColumnSet(columns);
			}

			return organizationService.Retrieve(entityReference.LogicalName, entityReference.Id, columnSet);
		}

		public static Entity Clone(this Entity original, params string[] forbiddenAttributes)
		{
			var clone = new Entity(original.LogicalName);
			foreach (var attribute in original.Attributes)
			{
				if (!CloneSettings.IsForbidden(original, attribute.Key, forbiddenAttributes))
					clone[attribute.Key] = attribute.Value;
			}
			return clone;
		}

		public static class CloneSettings
		{
			private static readonly List<string> ForbiddenAttributes = new List<string>();

			static CloneSettings()
			{
				ForbiddenAttributes.AddRange(new[]
				{
					"statecode",
					"statuscode",
					"ownerid",
					"owningbusinessunit",
					"owningteam",
					"owninguser",
					"createdon",
					"createdby",
					"modifiedon",
					"modifiedby"
				});
			}


			public static bool IsForbidden(Entity original, string propertyName, string[] otherForbiddenAttributes)
			{
				if (otherForbiddenAttributes == null) otherForbiddenAttributes = new string[0];
				if (string.Equals(propertyName, original.LogicalName + "id", StringComparison.OrdinalIgnoreCase)) return true;
				if (ForbiddenAttributes.Any(x => string.Equals(x, propertyName, StringComparison.OrdinalIgnoreCase))) return true;
				if (otherForbiddenAttributes.Any(x => string.Equals(x, propertyName, StringComparison.OrdinalIgnoreCase))) return true;
				return false;
			}
		}

		/// <summary>
		/// Creates a clone of the specified entity without field values (copies only LogicalName and Id)
		/// </summary>
		/// <param name="source">The entity to clone.</param>
		/// <returns>An empty copy of the current entity.</returns>
		public static Entity CloneEmpty(this Entity source)
		{
			var target = new Entity(source.LogicalName) {Id = source.Id};
			return target;
		}

		public static T GetAliasedValue<T>(this Entity entity, string attributeLogicalName)
		{
			if (null == entity.Attributes) { entity.Attributes = new AttributeCollection(); }

			var value = entity.GetAttributeValue<AliasedValue>(attributeLogicalName);
			if (value == null) return default(T);

			return (T)value.Value;
		}

		public static T GetAliasedValue<T>(this Entity entity, string attributeLogicalName, string alias)
		{
			return GetAliasedValue<T>(entity, $"{alias}.{attributeLogicalName}");
		}

		public static TProp Try<T, TProp>(this T item, Func<T, TProp> propertyAccessor, TProp defaultValue = default(TProp))
		{
			return Equals(item, default(T)) ? defaultValue : propertyAccessor(item);
		}

		public static void Assign(this IOrganizationService service, EntityReference Assignee, EntityReference Target)
		{
			var assignRequest = new AssignRequest
			{
				Assignee = Assignee,
				Target = Target
			};

			service.Execute(assignRequest);
		}
	}
}
