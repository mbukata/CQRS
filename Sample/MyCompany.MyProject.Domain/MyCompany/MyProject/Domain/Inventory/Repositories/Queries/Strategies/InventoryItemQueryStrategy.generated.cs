﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#region Copyright
// -----------------------------------------------------------------------
// <copyright company="cdmdotnet Limited">
//     Copyright cdmdotnet Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion
using Cqrs.Domain;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Cqrs.Authentication;
using Cqrs.Repositories.Queries;

namespace MyCompany.MyProject.Domain.Inventory.Repositories.Queries.Strategies
{
	[GeneratedCode("CQRS UML Code Generator", "1.500.497.383")]
	public partial class InventoryItemQueryStrategy : QueryStrategy, IInventoryItemQueryStrategy
	{
		protected IAuthenticationTokenHelper<System.Guid> AuthenticationTokenHelper { get; private set; }

		internal IList<Func<int, InventoryItemQueryStrategy>> SortingList { get; set; }

		public InventoryItemQueryStrategy(IAuthenticationTokenHelper<System.Guid> authenticationTokenHelper)
		{
			AuthenticationTokenHelper = authenticationTokenHelper;
			SortingList = new List<Func<int, InventoryItemQueryStrategy>>();
		}

		public virtual InventoryItemQueryStrategy WithRsn(Guid rsn)
		{
			QueryPredicate = And(IsNotLogicallyDeleted());
			QueryPredicate = And(WithPermissionScopeUser(AuthenticationTokenHelper.GetAuthenticationToken()));
			QueryPredicate = And(BuildQueryPredicate(WithRsn, rsn));
	
			OnWithRsn(rsn);

			return this;
		}

		partial void OnWithRsn(Guid rsn);

	}
}