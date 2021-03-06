﻿namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This abstract class is here for convenience, so as not to clutter up
    ///     your classes with property implementations.
    ///     The interface is what is used by datastore because
    ///     the benefit of an interface over an abstract class is you can't sneak logic into it.
    ///     e.g. property which may not serialize reliably, or constructor logic which affects field values.
    ///     Furthermore, if you expose add any logic to the base class, even that which is
    ///     serialisation safe, if a client has models assemblies each with a different version
    ///     of this logic, your code could start producing unexpected results.
    ///     So, NO LOGIC of any kind in these abstract classes.
    /// </summary>
    public abstract class Aggregate : Entity, IAggregate
    {
        protected Aggregate()
        {
            //These properties are set here when they could be set in Create because a lot of the tests which
            //create classes without create depend on these defaults and it is a significant convenience for it
            //to be set correctly by default.
            Active = true;
        }

        public bool Active { get; set; }

        public bool ReadOnly { get; set; }

        public List<IScopeReference> ScopeReferences { get; set; }

        public DateTime Modified { get; set; }

        public double ModifiedAsMillisecondsEpochTime { get; set; }

        public const string PartitionKeyValue = "shared";

        public string PartitionKey { get; set; } = PartitionKeyValue;
    }
}