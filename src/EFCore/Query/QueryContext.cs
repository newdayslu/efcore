// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The principal data structure used by a compiled query during execution.
    /// </summary>
    public abstract class QueryContext : IParameterValues
    {
        private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();
        private IStateManager _stateManager;

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="QueryContext" /> instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        protected QueryContext(
            [NotNull] QueryContextDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Gets the current DbContext.
        /// </summary>
        public virtual DbContext Context => Dependencies.CurrentContext.Context;

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual QueryContextDependencies Dependencies { get; }

        /// <summary>
        ///     Sets the navigation as loaded.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <param name="navigation"> The navigation property. </param>
        public virtual void SetNavigationIsLoaded([NotNull] object entity, [NotNull] INavigation navigation)
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(navigation, nameof(navigation));

            _stateManager.TryGetEntry(entity).SetIsLoaded(navigation);
        }

        /// <summary>
        ///     The query provider.
        /// </summary>
        /// <value>
        ///     The query provider.
        /// </value>
        public virtual IQueryProvider QueryProvider
            => Dependencies.QueryProvider;

        /// <summary>
        ///     The execution strategy factory.
        /// </summary>
        /// <value>
        ///     The execution strategy factory.
        /// </value>
        public virtual IExecutionStrategyFactory ExecutionStrategyFactory
            => Dependencies.ExecutionStrategyFactory;

        /// <summary>
        ///     Gets the concurrency detector.
        /// </summary>
        /// <value>
        ///     The concurrency detector.
        /// </value>
        public virtual IConcurrencyDetector ConcurrencyDetector
            => Dependencies.ConcurrencyDetector;

        /// <summary>
        ///     Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///     The cancellation token.
        /// </value>
        public virtual CancellationToken CancellationToken { get; set; }

        /// <summary>
        ///     Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///     The cancellation token.
        /// </value>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger
            => Dependencies.CommandLogger;

        /// <summary>
        ///     Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///     The cancellation token.
        /// </value>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> QueryLogger
            => Dependencies.QueryLogger;

        /// <summary>
        ///     The parameter values.
        /// </summary>
        public virtual IReadOnlyDictionary<string, object> ParameterValues
            => (IReadOnlyDictionary<string, object>)_parameterValues;

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="value"> The value. </param>
        public virtual void AddParameter(string name, object value)
        {
            Check.NotEmpty(name, nameof(name));

            _parameterValues.Add(name, value);
        }

        /// <summary>
        ///     Initializes the <see cref="IStateManager"/> to be used with this QueryContext.
        /// </summary>
        /// <param name="standAlone"> Whether a stand-alone <see cref="IStateManager"/> should be created to perform identity resolution. </param>
        public virtual void InitializeStateManager(bool standAlone = false)
        {
            if (_stateManager != null)
            {
                throw new InvalidOperationException(CoreStrings.QueryContextAlreadyInitializedStateManager);
            }

            _stateManager = standAlone
                ? new StateManager(Dependencies.StateManager.Dependencies)
                : Dependencies.StateManager;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual InternalEntityEntry TryGetEntry(
            [NotNull] IKey key,
            [NotNull] object[] keyValues,
            bool throwOnNullKey,
            out bool hasNullKey)
            => _stateManager.TryGetEntry(key, keyValues, throwOnNullKey, out hasNullKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual InternalEntityEntry StartTracking(
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            ValueBuffer valueBuffer)
            => _stateManager.StartTrackingFromQuery(entityType, entity, valueBuffer);
    }
}
