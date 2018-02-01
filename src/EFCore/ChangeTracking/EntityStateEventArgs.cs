// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Event arguments for the <see cref="ChangeTracker.EnityStateChanging" />
    ///     and <see cref="ChangeTracker.EnityStateChanged" /> events.
    /// </summary>
    public class EntityStateEventArgs : EventArgs
    {
        private readonly InternalEntityEntry _internalEntityEntry;
        private EntityEntry _entry;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityStateEventArgs(
            [NotNull] InternalEntityEntry internalEntityEntry,
            EntityState oldState,
            EntityState newState,
            bool fromQuery)
        {
            _internalEntityEntry = internalEntityEntry;
            OldState = oldState;
            NewState = newState;
            FromQuery = fromQuery;
        }

        /// <summary>
        ///     The <see cref="EntityEntry" /> for the entity who's state has changed.
        /// </summary>
        public virtual EntityEntry Entry => _entry ?? (_entry = new EntityEntry(_internalEntityEntry));

        /// <summary>
        ///     The state that the entity is transitioning from.
        /// </summary>
        public virtual EntityState OldState { get; }

        /// <summary>
        ///     The state that the entity is transitioning to.
        /// </summary>
        public virtual EntityState NewState { get; }

        /// <summary>
        ///     <c>True</c> if the change is for an entity that has just been queried from the database; <c>false</c> otherwise.
        /// </summary>
        public virtual bool FromQuery { get; }
    }
}
