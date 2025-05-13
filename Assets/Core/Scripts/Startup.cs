using Leopotam.EcsLite;
using UnityEngine;
using AB_Utility.FromSceneToEntityConverter;

namespace Client {
    public sealed class Startup : MonoBehaviour 
    {
        private EcsWorld _world;        
        private EcsWorld _eventWorld;        
        private IEcsSystems _updateSystems;
        private IEcsSystems _fixedUpdateSystems;
        private IEcsSystems _initSystems;

        private void Start () 
        {
            _world = new EcsWorld ();
            _eventWorld = new EcsWorld ();
            _initSystems = new EcsSystems(_world);
            _updateSystems = new EcsSystems (_world);
            _fixedUpdateSystems = new EcsSystems(_world);
            
            AddInitSystems();
            AddRunSystems();

            InjectAllSystems(_initSystems, _updateSystems, _fixedUpdateSystems);
            AddEventsDestroyer();
            
            _initSystems.Init();
            _fixedUpdateSystems.Init();
            _updateSystems.ConvertScene().Init();
        }

        private void Update () 
        {
            _updateSystems?.Run ();
        }
        
        private void FixedUpdate() 
        {
            _fixedUpdateSystems?.Run();
        }

        private void AddInitSystems()
        {
            // _initSystems
            //     .AddWorld(_eventWorld, "events")
            //     .Add(new InitSystemTest())
            //     ;
        }
        
        private void AddRunSystems() 
        {
            // _updateSystems
            //     .AddWorld(_eventWorld, "events")
            //     .Add(new RunSystemTest())
            //     ;
        }

        private void OnDestroy () 
        {
            _updateSystems?.Destroy ();
            _updateSystems = null;

            _fixedUpdateSystems?.Destroy();
            _fixedUpdateSystems = null;

            _world?.Destroy ();
            _world = null;
        }

        private void AddEventsDestroyer()
        {
            
        }

        private void InjectAllSystems(params IEcsSystems[] systems)
        {
            foreach (var system in systems)
            {
                
            }
        }
    }
}