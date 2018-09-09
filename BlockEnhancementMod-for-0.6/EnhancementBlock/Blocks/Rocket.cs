﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Common;
using Modding.Levels;

namespace BlockEnhancementMod.Blocks
{
    class RocketScript : EnhancementBlock
    {
        //General setting
        MToggle GuidedRocketToggle;
        MKey LockTargetKey;
        private Texture2D rocketAim;
        public Transform target;
        public TimedRocket rocket;
        public Rigidbody rocketRigidbody;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        //Networking setting
        private bool receivedRayFromClient = false;
        private Ray rayFromClient;

        //No smoke mode related
        MToggle NoSmokeToggle;
        private bool noSmoke = false;
        private bool smokeStopped = false;

        //Firing record related setting
        private bool targetHit = false;
        private float randomDelay = 0;
        private float fireTime = 0f;
        private bool fireTimeRecorded = false;

        //Guide related setting
        MSlider GuidedRocketTorqueSlider;
        MToggle GuidedRocketStabilityToggle;
        public bool guidedRocketStabilityOn = true;
        public bool guidedRocketActivated = false;
        public float torque = 100f;
        private readonly float maxTorque = 10000;
        private HashSet<Transform> explodedTarget = new HashSet<Transform>();
        private List<Collider> colliders = new List<Collider>();

        //Active guide related setting
        MSlider ActiveGuideRocketSearchAngleSlider;
        MKey SwitchGuideModeKey;
        public List<KeyCode> switchGuideModeKey = new List<KeyCode> { KeyCode.RightShift };
        public float searchAngle = 10;
        private readonly float safetyRadius = 15f;
        private readonly float maxSearchAngle = 25f;
        private readonly float maxSearchAngleNo8 = 90f;
        private bool activeGuide = true;
        private bool targetAquired = false;
        private bool searchStarted = false;

        //proximity fuze related setting
        MToggle ProximityFuzeToggle;
        MSlider ProximityFuzeRangeSlider;
        MSlider ProximityFuzeAngleSlider;
        public bool proximityFuzeActivated = false;
        public float proximityRange = 0f;
        public float proximityAngle = 0f;

        //Guide delay related setting
        MSlider GuideDelaySlider;
        public float guideDelay = 0f;
        private bool canTrigger = false;

        //High power explosion related setting
        MToggle HighExploToggle;
        public bool highExploActivated = false;
        private bool bombHasExploded = false;
        private readonly int levelBombCategory = 4;
        private readonly int levelBombID = 5001;
        private float bombExplosiveCharge = 0;
        private float explosiveCharge = 0f;
        private readonly float radius = 7f;
        private readonly float power = 3600f;
        private readonly float torquePower = 100000f;
        private readonly float upPower = 0.25f;

        private void MessageInitialisation()
        {
            ModNetworking.Callbacks[Messages.rocketTargetBlockBehaviourMsg] += (Message msg) =>
            {
                Debug.Log("Receive block target");
                target = ((Block)msg.GetData(0)).GameObject.transform;
            };
            ModNetworking.Callbacks[Messages.rocketTargetEntityMsg] += (Message msg) =>
            {
                Debug.Log("Receive entity target");
                target = ((Entity)msg.GetData(0)).GameObject.transform;
            };
            ModNetworking.Callbacks[Messages.rocketRayToHostMsg] += (Message msg) =>
            {
                rayFromClient = new Ray((Vector3)msg.GetData(0), (Vector3)msg.GetData(1));
                activeGuide = false;
                receivedRayFromClient = true;
            };
        }


        public override void SafeAwake()
        {
            //Load aim pic
            rocketAim = new Texture2D(256, 256);
            rocketAim.LoadImage(ModIO.ReadAllBytes("Resources\\Square-Red.png"));
            //Key mapper setup
            GuidedRocketToggle = BB.AddToggle(LanguageManager.trackTarget, "TrackingRocket", guidedRocketActivated);
            GuidedRocketToggle.Toggled += (bool value) =>
            {
                guidedRocketActivated =
                GuidedRocketTorqueSlider.DisplayInMapper =
                ProximityFuzeToggle.DisplayInMapper =
                LockTargetKey.DisplayInMapper =
                SwitchGuideModeKey.DisplayInMapper =
                ActiveGuideRocketSearchAngleSlider.DisplayInMapper =
                GuideDelaySlider.DisplayInMapper =
                GuidedRocketStabilityToggle.DisplayInMapper =
                NoSmokeToggle.DisplayInMapper =
                value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketActivated = GuidedRocketToggle.IsActive; };

            ProximityFuzeToggle = BB.AddToggle(LanguageManager.proximityFuze, "ProximityFuze", proximityFuzeActivated);
            ProximityFuzeToggle.Toggled += (bool value) =>
            {
                proximityFuzeActivated =
                ProximityFuzeRangeSlider.DisplayInMapper =
                ProximityFuzeAngleSlider.DisplayInMapper =
                value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { proximityFuzeActivated = ProximityFuzeToggle.IsActive; };

            NoSmokeToggle = BB.AddToggle(LanguageManager.noSmoke, "NoSmoke", noSmoke);
            NoSmokeToggle.Toggled += (bool value) =>
            {
                noSmoke = value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { noSmoke = NoSmokeToggle.IsActive; };

            HighExploToggle = BB.AddToggle(LanguageManager.highExplo, "HighExplo", highExploActivated);
            HighExploToggle.Toggled += (bool value) =>
            {
                highExploActivated = value;
                ChangedProperties();
            };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { highExploActivated = HighExploToggle.IsActive; };

            ActiveGuideRocketSearchAngleSlider = BB.AddSlider(LanguageManager.searchAngle, "searchAngle", searchAngle, 0, maxSearchAngle);
            ActiveGuideRocketSearchAngleSlider.ValueChanged += (float value) => { searchAngle = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { searchAngle = ActiveGuideRocketSearchAngleSlider.Value; };

            ProximityFuzeRangeSlider = BB.AddSlider(LanguageManager.closeRange, "closeRange", proximityRange, 0, 10);
            ProximityFuzeRangeSlider.ValueChanged += (float value) => { proximityRange = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { proximityRange = ProximityFuzeRangeSlider.Value; };

            ProximityFuzeAngleSlider = BB.AddSlider(LanguageManager.closeAngle, "closeAngle", proximityAngle, 0, 90);
            ProximityFuzeAngleSlider.ValueChanged += (float value) => { proximityAngle = value; ChangedProperties(); };
            ////BlockDataLoadEvent += (XDataHolder BlockData) => { proximityAngle = ProximityFuzeAngleSlider.Value; };

            GuidedRocketTorqueSlider = BB.AddSlider(LanguageManager.torqueOnRocket, "torqueOnRocket", torque, 0, 100);
            GuidedRocketTorqueSlider.ValueChanged += (float value) => { torque = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { torque = GuidedRocketTorqueSlider.Value; };

            GuidedRocketStabilityToggle = BB.AddToggle(LanguageManager.rocketStability, "RocketStabilityOn", guidedRocketStabilityOn);
            GuidedRocketStabilityToggle.Toggled += (bool value) => { guidedRocketStabilityOn = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { guidedRocketStabilityOn = GuidedRocketStabilityToggle.IsActive; };

            GuideDelaySlider = BB.AddSlider(LanguageManager.guideDelay, "guideDelay", guideDelay, 0, 2);
            GuideDelaySlider.ValueChanged += (float value) => { guideDelay = value; ChangedProperties(); };
            //BlockDataLoadEvent += (XDataHolder BlockData) => { guideDelay = GuideDelaySlider.Value; };

            LockTargetKey = BB.AddKey(LanguageManager.lockTarget, "lockTarget", KeyCode.Delete);
            LockTargetKey.InvokeKeysChanged();

            SwitchGuideModeKey = BB.AddKey(LanguageManager.switchGuideMode, "ActiveSearchKey", KeyCode.RightShift);
            SwitchGuideModeKey.InvokeKeysChanged();

            //Add reference to TimedRocket
            rocket = gameObject.GetComponent<TimedRocket>();
            rocketRigidbody = gameObject.GetComponent<Rigidbody>();

            //Initialise messages
            MessageInitialisation();

#if DEBUG
            //ConsoleController.ShowMessage("火箭添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            GuidedRocketToggle.DisplayInMapper = value;
            HighExploToggle.DisplayInMapper = value;
            NoSmokeToggle.DisplayInMapper = value;
            SwitchGuideModeKey.DisplayInMapper = value && guidedRocketActivated;
            ActiveGuideRocketSearchAngleSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketTorqueSlider.DisplayInMapper = value && guidedRocketActivated;
            GuidedRocketStabilityToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeToggle.DisplayInMapper = value && guidedRocketActivated;
            ProximityFuzeRangeSlider.DisplayInMapper = value && proximityFuzeActivated;
            ProximityFuzeAngleSlider.DisplayInMapper = value && proximityFuzeActivated;
            GuideDelaySlider.DisplayInMapper = value && guidedRocketActivated;
            LockTargetKey.DisplayInMapper = value && guidedRocketActivated && guidedRocketActivated;
        }

        public override void OnSimulateStart()
        {
            smokeStopped = false;
            if (guidedRocketActivated)
            {
                // Initialisation for simulation
                fireTimeRecorded = canTrigger = targetAquired = searchStarted = targetHit = bombHasExploded = receivedRayFromClient = false;
                activeGuide = true;
                target = null;
                searchAngle = Mathf.Clamp(searchAngle, 0, No8Workshop ? maxSearchAngleNo8 : maxSearchAngle);
                explodedTarget.Clear();
                StopAllCoroutines();

                // Read the charge from rocket
                explosiveCharge = bombExplosiveCharge = rocket.ChargeSlider.Value;

                // Make sure the high explo mode is not too imba
                if (highExploActivated && !No8Workshop)
                {
                    bombExplosiveCharge = Mathf.Clamp(explosiveCharge, 0f, 1.5f);
                }
            }
        }

        public override void SimulateUpdateAlways()
        {
            if (guidedRocketActivated)
            {
                //When toggle auto aim key is released, change the auto aim status
                if (SwitchGuideModeKey.IsReleased)
                {
                    activeGuide = !activeGuide;
                    if (!activeGuide)
                    {
                        target = null;
                    }
                    else
                    {
                        targetAquired = false;
                    }
                }

                //if (StatMaster.isHosting && receivedRayFromClient)
                //{
                //    Debug.Log("Should not see this message in client");
                //    receivedRayFromClient = false;
                //    //Find targets in the manual search mode by casting a sphere along the ray
                //    float manualSearchRadius = 1.25f;
                //    RaycastHit[] hits = Physics.SphereCastAll(rayFromClient, manualSearchRadius, Mathf.Infinity);

                //    for (int i = 0; i < hits.Length; i++)
                //    {
                //        if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                //        {
                //            target = hits[i].transform;
                //            break;
                //        }
                //    }
                //    if (target == null)
                //    {
                //        for (int i = 0; i < hits.Length; i++)
                //        {
                //            if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
                //            {
                //                target = hits[i].transform;
                //                break;
                //            }
                //        }
                //    }
                //    SendTargetToClient();
                //}

                if (LockTargetKey.IsReleased)
                {
                    target = null;
                    if (activeGuide)
                    {
                        //When launch key is released, reset target search
                        if (rocket.hasFired)
                        {
                            targetAquired = searchStarted = false;
                            RocketRadarSearch();
                        }
                    }
                    else
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (StatMaster.isClient)
                        {
                            SendRayToHost(ray);
                        }
                        else
                        {
                            //Find targets in the manual search mode by casting a sphere along the ray
                            float manualSearchRadius = 1.25f;
                            RaycastHit[] hits = Physics.SphereCastAll(receivedRayFromClient ? rayFromClient : ray, manualSearchRadius, Mathf.Infinity);
                            Physics.Raycast(receivedRayFromClient ? rayFromClient : ray, out RaycastHit rayHit);

                            for (int i = 0; i < hits.Length; i++)
                            {
                                if (hits[i].transform.gameObject.GetComponent<BlockBehaviour>())
                                {
                                    target = hits[i].transform;
                                    break;
                                }
                            }
                            if (target == null)
                            {
                                for (int i = 0; i < hits.Length; i++)
                                {
                                    if (hits[i].transform.gameObject.GetComponent<LevelEntity>())
                                    {
                                        target = hits[i].transform;
                                        break;
                                    }
                                }
                            }
                            if (target == null && !StatMaster.isMP)
                            {
                                target = rayHit.transform;
                            }
                            if (receivedRayFromClient)
                            {
                                SendTargetToClient();
                            }
                            receivedRayFromClient = false;
                        }
                    }
                }
            }
        }

        public override void SimulateFixedUpdateAlways()
        {
            if (rocket.hasFired && !rocket.hasExploded)
            {
                //If no smoke mode is enabled, stop all smoke
                if (noSmoke && !smokeStopped)
                {
                    foreach (var smoke in rocket.trail)
                    {
                        smoke.Stop();
                    }
                    smokeStopped = true;
                }

                if (guidedRocketActivated)
                {
                    //Add aerodynamic force to rocket
                    AddResistancePerpendicularToRocketVelocity();

                    //Record the launch time for the guide delay
                    if (!fireTimeRecorded)
                    {
                        fireTimeRecorded = true;
                        fireTime = Time.time;
                        randomDelay = UnityEngine.Random.Range(0f, 0.1f);
                    }

                    //If rocket is burning, explode it
                    if (highExploActivated && rocket.gameObject.GetComponent<FireTag>().burning)
                    {
                        RocketExplode();
                    }

                    //Rocket can be triggered after the time elapsed after firing is greater than guide delay
                    if (Time.time - fireTime >= guideDelay + randomDelay && !canTrigger)
                    {
                        canTrigger = true;
                    }

                    //Check if target is no longer valuable (lazy check)
                    if (target != null)
                    {
                        try
                        {
                            if (target.gameObject.GetComponent<FireTag>().burning)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<TimedRocket>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ExplodeOnCollide>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                        try
                        {
                            if (target.gameObject.GetComponent<ControllableBomb>().hasExploded)
                            {
                                explodedTarget.Add(target);
                                target = null;
                                targetAquired = false;
                            }
                        }
                        catch { }
                    }
                    //If no target when active guide, search for a new target
                    if (activeGuide && !targetAquired)
                    {
                        RocketRadarSearch();
                    }
                }
            }
        }

        public override void SimulateLateUpdateAlways()
        {
            if (guidedRocketActivated && rocket.hasFired && !rocket.hasExploded)
            {
                if (target != null && canTrigger)
                {
                    // Calculating the rotating axis
                    Vector3 velocity = Vector3.zero;
                    try
                    {
                        velocity = target.gameObject.GetComponent<Rigidbody>().velocity;
                    }
                    catch { }
                    //Add position prediction
                    Vector3 positionDiff = target.position + velocity * Time.fixedDeltaTime - BB.CenterOfBounds;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);
                    bool forward = Vector3.Dot(transform.up, positionDiff) > 0;
                    Vector3 rotatingAxis = -Vector3.Cross(positionDiff.normalized, transform.up);

                    //Add torque to the rocket based on the angle difference
                    //If in auto guide mode, the rocket will restart searching when target is out of sight
                    //else, apply maximum torque to the rocket
                    if (forward && angleDiff <= searchAngle)
                    {
                        try { rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * ((-Mathf.Pow(angleDiff / maxSearchAngleNo8 - 1f, 2) + 1)) * rotatingAxis);
                        }
                        catch { }
                    }
                    else
                    {
                        if (!activeGuide)
                        {
                            try { rocketRigidbody.AddTorque(Mathf.Clamp(torque, 0, 100) * maxTorque * rotatingAxis); }
                            catch { }
                        }
                        else
                        {
                            targetAquired = false;
                        }
                    }
                    //If proximity fuse is enabled, the rocket will explode when target is in preset range&angle
                    if (proximityFuzeActivated && positionDiff.magnitude <= proximityRange && angleDiff >= proximityAngle)
                    {
                        RocketExplode();
                    }
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            try
            {
                if (rocket.isSimulating && !rocket.hasExploded && collision.gameObject.name.Contains("CanonBall"))
                {
                    rocket.OnExplode();
                }
            }
            catch { }
            if (rocket.hasFired && collision.impulse.magnitude > 1 && canTrigger)
            {
                RocketExplode();
            }
        }
        void OnCollisionStay(Collision collision)
        {
            try
            {
                if (rocket.isSimulating && !rocket.hasExploded && collision.gameObject.name.Contains("CanonBall"))
                {
                    rocket.OnExplode();
                }
            }
            catch { };
            if (rocket.hasFired && collision.impulse.magnitude > 1 && canTrigger)
            {
                RocketExplode();
            }
        }

        private void RocketExplode()
        {
            //Reset some parameter and set the rocket to explode
            //Stop the search target coroutine
            searchStarted = targetHit = true;
            StopCoroutine(SearchForTarget());

            if (!highExploActivated)
            {
                if (!rocket.hasExploded) rocket.OnExplode();
            }
            else
            {
                if (!bombHasExploded && explosiveCharge != 0)
                {
                    if (StatMaster.isHosting)
                    {
                        SendExplosionPositionToAll();
                    }
                    bombHasExploded = true;
                    //Generate a bomb from level editor and let it explode
                    try
                    {
                        GameObject bomb = (GameObject)Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject, rocket.transform.position, rocket.transform.rotation);
                        ExplodeOnCollide bombControl = bomb.GetComponent<ExplodeOnCollide>();
                        bomb.transform.localScale = Vector3.one * bombExplosiveCharge;
                        bombControl.radius = radius * bombExplosiveCharge;
                        bombControl.power = power * bombExplosiveCharge;
                        bombControl.torquePower = torquePower * bombExplosiveCharge;
                        bombControl.upPower = upPower;
                        bombControl.Explodey();
                    }
                    catch { }

                    //Add explode and ignition effects to the affected objects
                    try
                    {
                        Collider[] hits = Physics.OverlapSphere(rocket.transform.position, radius * bombExplosiveCharge);
                        foreach (var hit in hits)
                        {
                            if (hit.attachedRigidbody != null && hit.attachedRigidbody.gameObject.layer != 22)
                            {
                                try
                                {
                                    if (hit.attachedRigidbody.gameObject.GetComponent<RocketScript>()) continue;
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.WakeUp();
                                    hit.attachedRigidbody.constraints = RigidbodyConstraints.None;
                                    hit.attachedRigidbody.AddExplosionForce(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                    hit.attachedRigidbody.AddRelativeTorque(UnityEngine.Random.insideUnitSphere.normalized * torquePower * bombExplosiveCharge);
                                }
                                catch { }

                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<FireTag>().Ignite();
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<ExplodeMultiplier>().Explodey(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<SimpleBirdAI>().Explode();
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<EnemyAISimple>().Die();
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<CastleWallBreak>().BreakExplosion(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<BreakOnForce>().BreakExplosion(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(power * bombExplosiveCharge, rocket.transform.position, radius * bombExplosiveCharge, upPower);
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<InjuryController>().activeType = InjuryType.Fire;
                                    hit.attachedRigidbody.gameObject.GetComponent<InjuryController>().Kill();
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                    if (!rocket.hasExploded)
                    {
                        rocket.OnExplode();
                    }
                }

            }
        }

        private void RocketRadarSearch()
        {
            if (!searchStarted && activeGuide)
            {
                searchStarted = true;
                StopCoroutine(SearchForTarget());
                StartCoroutine(SearchForTarget());
            }
        }

        IEnumerator SearchForTarget()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, 0.1f));
            //Grab every machine block at the start of search
            HashSet<Machine.SimCluster> simClusters = new HashSet<Machine.SimCluster>();

            if (StatMaster.isMP)
            {
                foreach (var player in Playerlist.Players)
                {
                    if (!player.isSpectator)
                    {
                        if (player.machine.isSimulating && !player.machine.LocalSim && player.machine.PlayerID != rocket.ParentMachine.PlayerID)
                        {
                            if (rocket.Team == MPTeam.None || rocket.Team != player.team)
                            {
                                simClusters.UnionWith(player.machine.simClusters);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var cluster in Machine.Active().simClusters)
                {
                    if ((cluster.Base.transform.position - rocket.Position).magnitude > safetyRadius)
                    {
                        simClusters.Add(cluster);
                    }
                }
            }

            //Iternating the list to find the target that satisfy the conditions
            while (!targetAquired && !targetHit && simClusters.Count > 0)
            {
                HashSet<Machine.SimCluster> simClusterForSearch = new HashSet<Machine.SimCluster>(simClusters);
                HashSet<Machine.SimCluster> unwantedClusters = new HashSet<Machine.SimCluster>();

                foreach (var cluster in simClusters)
                {
                    Vector3 positionDiff = cluster.Base.gameObject.transform.position - rocket.CenterOfBounds;
                    float angleDiff = Vector3.Angle(positionDiff.normalized, transform.up);
                    bool forward = Vector3.Dot(positionDiff, transform.up) > 0;
                    bool skipCluster = !(forward && angleDiff < searchAngle) || ShouldSkipCluster(cluster.Base);

                    if (!skipCluster)
                    {
                        foreach (var block in cluster.Blocks)
                        {
                            skipCluster = ShouldSkipCluster(block);
                            if (skipCluster)
                            {
                                break;
                            }
                        }
                    }
                    if (skipCluster)
                    {
                        unwantedClusters.Add(cluster);
                    }
                }

                simClusterForSearch.ExceptWith(unwantedClusters);

                if (simClusterForSearch.Count > 0)
                {
                    target = GetMostValuableBlock(simClusterForSearch);
                    targetAquired = true;
                    searchStarted = false;
                    SendTargetToClient();
                    StopCoroutine(SearchForTarget());
                }
                yield return null;
            }
        }

        private Transform GetMostValuableBlock(HashSet<Machine.SimCluster> simClusterForSearch)
        {
            //Search for any blocks within the search radius for every block in the hitlist
            int[] targetValue = new int[simClusterForSearch.Count];
            Machine.SimCluster[] clusterArray = new Machine.SimCluster[simClusterForSearch.Count];
            List<Machine.SimCluster> maxClusters = new List<Machine.SimCluster>();

            //Start searching
            int i = 0;
            foreach (var simCluster in simClusterForSearch)
            {
                int clusterValue = simCluster.Blocks.Length + 1;
                clusterValue = CalculateClusterValue(simCluster.Base, clusterValue);
                foreach (var block in simCluster.Blocks)
                {
                    clusterValue = CalculateClusterValue(block, clusterValue);
                }
                targetValue[i] = clusterValue;
                clusterArray[i] = simCluster;
                i++;
            }
            //Find the block that has the max number of blocks around it
            //If there are multiple withh the same highest value, randomly return one of them
            int maxValue = targetValue.Max();
            for (i = 0; i < targetValue.Length; i++)
            {
                if (targetValue[i] == maxValue)
                {
                    maxClusters.Add(clusterArray[i]);
                }
            }

            int closestIndex = 0;
            float distanceMin = Mathf.Infinity;

            for (i = 0; i < maxClusters.Count; i++)
            {
                float distanceCurrent = (maxClusters[i].Base.gameObject.transform.position - rocket.transform.position).magnitude;
                if (distanceCurrent < distanceMin)
                {
                    closestIndex = i;
                    distanceMin = distanceCurrent;
                }
            }

            return maxClusters[closestIndex].Base.gameObject.transform;
        }

        private void AddResistancePerpendicularToRocketVelocity()
        {
            Vector3 locVel = transform.InverseTransformDirection(rocketRigidbody.velocity);
            Vector3 dir = new Vector3(0.1f, 0f, 0.1f) * 0.5f;
            float velocitySqr = rocketRigidbody.velocity.sqrMagnitude;
            float currentVelocitySqr = Mathf.Min(velocitySqr, 30f);
            rocketRigidbody.AddRelativeForce(Vector3.Scale(dir, -locVel) * currentVelocitySqr);
        }

        private int CalculateClusterValue(BlockBehaviour block, int clusterValue)
        {
            //Some blocks weights more than others
            GameObject targetObj = block.gameObject;
            //A bomb
            if (targetObj.GetComponent<ExplodeOnCollideBlock>())
            {
                if (!targetObj.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                {
                    clusterValue *= 64;
                }
            }
            //A fired and unexploded rocket
            if (targetObj.GetComponent<TimedRocket>())
            {
                if (targetObj.GetComponent<TimedRocket>().hasFired && !targetObj.GetComponent<TimedRocket>().hasExploded)
                {
                    clusterValue *= 128;
                }
            }
            //A watering watercannon
            if (targetObj.GetComponent<WaterCannonController>())
            {
                if (targetObj.GetComponent<WaterCannonController>().isActive)
                {
                    clusterValue *= 16;
                }
            }
            //A flying flying-block
            if (targetObj.GetComponent<FlyingController>())
            {
                if (targetObj.GetComponent<FlyingController>().canFly)
                {
                    clusterValue *= 2;
                }
            }
            //A flaming flamethrower
            if (targetObj.GetComponent<FlamethrowerController>())
            {
                if (targetObj.GetComponent<FlamethrowerController>().isFlaming)
                {
                    clusterValue *= 8;
                }
            }
            //A spinning wheel/cog
            if (targetObj.GetComponent<CogMotorControllerHinge>())
            {
                if (targetObj.GetComponent<CogMotorControllerHinge>().Velocity != 0)
                {
                    clusterValue *= 2;
                }
            }
            return clusterValue;
        }

        private bool ShouldSkipCluster(BlockBehaviour block)
        {
            bool skipCluster = false;
            try
            {
                if (block.gameObject.GetComponent<FireTag>().burning)
                {
                    skipCluster = true;
                }
            }
            catch { }
            try
            {
                if (block.gameObject.GetComponent<TimedRocket>().hasExploded)
                {
                    skipCluster = true;
                }
            }
            catch { }
            try
            {
                if (block.gameObject.GetComponent<ExplodeOnCollideBlock>().hasExploded)
                {
                    skipCluster = true;
                }
            }
            catch { }
            try
            {
                if (block.gameObject.GetComponent<ControllableBomb>().hasExploded)
                {
                    skipCluster = true;
                }
            }
            catch { }

            return skipCluster;
        }

        private void OnGUI()
        {
            if (StatMaster.isMP && StatMaster.isHosting)
            {
                if (rocket.ParentMachine.PlayerID != Playerlist.Players[0].machine.PlayerID)
                {
                    return;
                }
            }
            DrawTargetRedSquare();
        }

        private void DrawTargetRedSquare()
        {
            if (target != null && !rocket.hasExploded && rocket.isSimulating && rocket != null)
            {
                int squareWidth = 16;
                Vector3 itemScreenPosition = Camera.main.WorldToScreenPoint(target.position);
                GUI.DrawTexture(new Rect(itemScreenPosition.x - squareWidth / 2, Camera.main.pixelHeight - itemScreenPosition.y - squareWidth / 2, squareWidth, squareWidth), rocketAim);
            }
        }

        private void SendTargetToClient()
        {
            if (target != null && rocket.ParentMachine.PlayerID != 0)
            {
                if (target.gameObject.GetComponent<BlockBehaviour>())
                {
                    Message targetBlockBehaviourMsg = Messages.rocketTargetBlockBehaviourMsg.CreateMessage(target.gameObject.GetComponent<BlockBehaviour>());
                    ModNetworking.SendTo(Player.GetAllPlayers()[rocket.ParentMachine.PlayerID], targetBlockBehaviourMsg);
                }
                if (target.gameObject.GetComponent<LevelEntity>())
                {
                    Message targetEntityMsg = Messages.rocketTargetEntityMsg.CreateMessage(target.gameObject.GetComponent<LevelEntity>());
                    ModNetworking.SendTo(Player.GetAllPlayers()[rocket.ParentMachine.PlayerID], targetEntityMsg);
                }
            }
        }

        private void SendRayToHost(Ray ray)
        {
            Message rayToHostMsg = Messages.rocketRayToHostMsg.CreateMessage(ray.origin, ray.direction);
            ModNetworking.SendToHost(rayToHostMsg);
        }

        private void SendExplosionPositionToAll()
        {
            Message explosionPositionMsg = Messages.rocketHighExploPosition.CreateMessage(rocket.transform.position, bombExplosiveCharge);
            ModNetworking.SendToAll(explosionPositionMsg);
        }
    }
}