using System;
namespace LazyStackDynamoDBRepo
{
    /// <summary>
    /// Each data object we store in the database must be 
    /// processed to move information from the data object of Type T
    /// into the the Envelope of Type TEnv.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEnv"></typeparam>
    public interface IEntityHelper<T,TEnv>
        where TEnv : class, IDYDBEnvelope
    {
        /// <summary>
        /// Instance holds the schema object of type T
        /// </summary>
        T Instance { get; set; }

        /// <summary>
        /// Update the Instance.CreateUtcTick value.
        /// Note that the Instance may not have a CreateUtcTick value or 
        /// it may be named differently and be stored in a DateTime format.
        /// SetCreateUtcTick handles these considerations. We recommend that
        /// all Instance items have CreateUtcTick information.
        /// </summary>
        /// <param name="t"></param>
        void SetCreateUtcTick(long t);

        /// <summary>
        /// Update the Instance.UpdateUtcTick value;
        /// Note that the Instance may not have a UpdateUtcTick value or 
        /// it may be named differently and be stored in a DateTime format.
        /// SetUpdateUtcTick handles these considerations. We recommend that
        /// all Instance items have UpdateUtcTick information.        
        /// </summary>
        /// <param name="t"></param>
        void SetUpdateUtcTick(long t);

        /// <summary>
        /// Get the Instance.UpdateUtcTick value;
        /// Note that the Instance may not have a UpdateUtcTick value or 
        /// it may be named differently and be stored in a DateTime format.
        /// GetUpdateUtcTick handles these considerations. We recommend that
        /// all Instance items have UpdateUtcTick information.
        /// </summary>
        /// <returns></returns>
        long GetUpdateUtcTick();

        /// <summary>
        /// Update the Envelope and optionally serialize the Instance into the Envelope's Data property.
        /// At a minimum extract values from the Instance to populate the Envelopes PK, SK.
        /// Optionally, use Instance values to populate SK1..SK5
        /// </summary>
        /// <param name="env">Envelope</param>
        /// <param name="dbEnvelope"></param>
        /// <param name="serialize">Serialize the Instance into the Envelope's Data property.</param>
        void UpdateEnvelope(TEnv env, TEnv dbEnvelope = null, bool serialize = false);

        /// <summary>
        /// Return an instance of type T from the envelopes Data property.
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        T Deserialize(TEnv env);
    }
}
