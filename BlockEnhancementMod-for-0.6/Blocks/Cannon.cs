﻿using System;
using UnityEngine;

namespace BlockEnhancementMod
{
    /// <summary>加农炮</summary>
    class Cannon : Block
    {

        public CannonScript cannonScript;

        public MSlider StrengthSlider;

        public MSlider IntervalSlider;

        public MSlider RandomDelaySlider;

        public MSlider KnockBackSpeedSlider;

        public MToggle BulletToggle;

        public MToggle InheritSizeToggle;

        public MSlider BulletMassSlider;

        public MSlider BulletDragSlider;

        public MToggle TrailToggle;

        public MSlider TrailLengthSlider;

        public MColourSlider TrailColorSlider;

        public float Strength = 1f;

        public float Interval = 0.25f;

        public float RandomDelay = 0.2f;

        public float KnockBackSpeed = 1f;

        public bool Bullet = false;

        public bool InheritSize = false;

        public float BulletMass = 2f;

        public float BulletDrag = 0.2f;

        public bool Trail = false;

        public float TrailLength = 1f;

        public Color TrailColor = Color.yellow;

        public Cannon(BlockBehaviour block) : base(block)
        {

            if (BB.GetComponent<CannonScript>() == null)
            {

                cannonScript = BB.gameObject.AddComponent<CannonScript>();

                //foreach (var s in BB.Sliders) { if (s.Key == "strength") { StrengthSlider = s; break; } }              
                //StrengthSlider.ValueChanged += (float value) => { Strength = value; ChangedPropertise(); };

                IntervalSlider = new MSlider("发射间隔", "Interval", Interval, 0f, 0.5f, false);
                IntervalSlider.ValueChanged += (float value) => { Interval = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(IntervalSlider);

                RandomDelaySlider = new MSlider("随机延迟", "RandomDelay", RandomDelay, 0f, 0.5f, false);
                RandomDelaySlider.ValueChanged += (float value) => { RandomDelay = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(RandomDelaySlider);

                KnockBackSpeedSlider = new MSlider("后坐力", "KnockBackSpeed", KnockBackSpeed, 0.2f, 1f, false);
                KnockBackSpeedSlider.ValueChanged += (float value) => { KnockBackSpeed = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(KnockBackSpeedSlider);

                BulletToggle = new MToggle("自定子弹", "Bullet", Bullet);
                BulletToggle.Toggled += (bool value) => { BulletDragSlider.DisplayInMapper = BulletMassSlider.DisplayInMapper = InheritSizeToggle.DisplayInMapper = Bullet = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(BulletToggle);

                InheritSizeToggle = new MToggle("尺寸继承", "InheritSize", InheritSize);
                InheritSizeToggle.Toggled += (bool value) => { InheritSize = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(InheritSizeToggle);

                BulletMassSlider = new MSlider("子弹质量", "BulletMass", BulletMass, 0.1f, 2f, false);
                BulletMassSlider.ValueChanged += (float value) => { BulletMass = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(BulletMassSlider);

                BulletDragSlider = new MSlider("子弹阻力", "BulletDrag", BulletDrag, 0.01f, 0.5f, false);
                BulletDragSlider.ValueChanged += (float value) => { BulletDrag = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(BulletDragSlider);

                TrailToggle = new MToggle("显示尾迹", "Trail", Trail);
                TrailToggle.Toggled += (bool value) => { Trail = TrailColorSlider.DisplayInMapper = TrailLengthSlider.DisplayInMapper = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(TrailToggle);

                TrailLengthSlider = new MSlider("尾迹长度", "trail length", TrailLength, 0.2f, 2f, false);
                TrailLengthSlider.ValueChanged += (float value) => { TrailLength = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(TrailLengthSlider);

                TrailColorSlider = new MColourSlider("尾迹颜色", "trail color", TrailColor, false);
                TrailColorSlider.ValueChanged += (Color value) => { TrailColor = value; ChangedPropertise(); };
                CurrentMapperTypes.Add(TrailColorSlider);
            }


            LoadConfiguration();

            ChangedPropertise();
            DisplayInMapper(EnhancementEnable);

            Controller.MapperTypesField.SetValue(block, CurrentMapperTypes);

#if DEBUG
            BesiegeConsoleController.ShowMessage("加农炮添加进阶属性");
#endif

        }

        public override void LoadConfiguration()
        {
            //base.LoadConfiguration();

            //if (Controller.MI == null)
            //{
            //    return;
            //}

            foreach (var blockinfo in Controller.MI.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    XDataHolder bd = blockinfo.BlockData;

                    ////Enhancement.IsActive = EnhancementEnable = Configuration.GetBool(Enhancement.Key, false);

                    ////if (bd.HasKey("bmt-" + StrengthSlider.Key)) { StrengthSlider.Value = Strength = bd.ReadFloat("bmt-" + StrengthSlider.Key); }
                    
                    //if (bd.HasKey("bmt-" + IntervalSlider.Key)) { IntervalSlider.Value = Interval = bd.ReadFloat("bmt-" + IntervalSlider.Key); }

                    //if (bd.HasKey("bmt-" + RandomDelaySlider.Key)) { RandomDelaySlider.Value = RandomDelay = bd.ReadFloat("bmt-" + RandomDelaySlider.Key); }

                    //if (bd.HasKey("bmt-" + KnockBackSpeedSlider.Key)) { KnockBackSpeedSlider.Value = KnockBackSpeed = bd.ReadFloat("bmt-" + KnockBackSpeedSlider.Key); }

                    //if (bd.HasKey("bmt-" + BulletToggle.Key)) { BulletToggle.IsActive = Bullet = bd.ReadBool("bmt-" + BulletToggle.Key); }

                    //if (bd.HasKey("bmt-" + InheritSizeToggle.Key)) { InheritSizeToggle.IsActive = InheritSize = bd.ReadBool("bmt-" + InheritSizeToggle.Key); }

                    //if (bd.HasKey("bmt-" + BulletMassSlider.Key)) { BulletMassSlider.Value = BulletMass = bd.ReadFloat("bmt-" + BulletMassSlider.Key); }

                    //if (bd.HasKey("bmt-" + BulletDragSlider.Key)) { BulletDragSlider.Value = BulletDrag = bd.ReadFloat("bmt-" + BulletDragSlider.Key); }

                    //if (bd.HasKey("bmt-" + TrailToggle.Key)) { TrailToggle.IsActive = Trail = bd.ReadBool("bmt-" + TrailToggle.Key); }

                    //if (bd.HasKey("bmt-" + TrailLengthSlider.Key)) { TrailLengthSlider.Value = TrailLength = bd.ReadFloat("bmt-" + TrailLengthSlider.Key); }

                    //if (bd.HasKey("bmt-" + TrailColorSlider.Key)) { TrailColorSlider.Value = TrailColor = bd.ReadColor("bmt-" + TrailColorSlider.Key); }


                    //break;
                }

            }



        }

        public override void SaveConfiguration(MachineInfo mi)
        {
            

            foreach (var blockinfo in mi.Blocks)
            {
                if (blockinfo.Guid == BB.Guid)
                {
                    //blockinfo.BlockData.Write("bmt-" + "shoot", new string[] { "E", "R" });
                    //blockinfo.BlockData.Write("bmt-" + StrengthSlider.Key, Strength);
                    blockinfo.BlockData.Write("bmt-" + IntervalSlider.Key, IntervalSlider.Value);

                    blockinfo.BlockData.Write("bmt-" + RandomDelaySlider.Key, RandomDelaySlider.Value);

                    blockinfo.BlockData.Write("bmt-" + KnockBackSpeedSlider.Key, KnockBackSpeedSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + BulletToggle.Key, BulletToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + InheritSizeToggle.Key, InheritSizeToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + BulletMassSlider.Key, BulletMassSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + BulletDragSlider.Key, BulletDragSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + TrailToggle.Key, TrailToggle.IsActive);
                    blockinfo.BlockData.Write("bmt-" + TrailLengthSlider.Key, TrailLengthSlider.Value);
                    blockinfo.BlockData.Write("bmt-" + TrailColorSlider.Key, TrailColorSlider.Value);

                    break;
                }

            }

            base.SaveConfiguration(mi);

        }

        public override void DisplayInMapper(bool value)
        {
            base.DisplayInMapper(value);


            IntervalSlider.DisplayInMapper = value;
            RandomDelaySlider.DisplayInMapper = value;
            KnockBackSpeedSlider.DisplayInMapper = value;
            BulletToggle.DisplayInMapper = value;
            InheritSizeToggle.DisplayInMapper = value && Bullet;
            BulletMassSlider.DisplayInMapper = value && Bullet;
            BulletDragSlider.DisplayInMapper = value && Bullet;

            TrailColorSlider.DisplayInMapper = Trail;
            TrailLengthSlider.DisplayInMapper = Trail;

        }

        public override void ChangedPropertise()
        {
            cannonScript.Strength = Strength;
            cannonScript.bBullet = Bullet;
            cannonScript.InheritSize = InheritSize;
            cannonScript.Interval = Interval;
            cannonScript.BulletMass = BulletMass;
            cannonScript.Drag = BulletDrag;
            cannonScript.RandomDelay = RandomDelay;
            cannonScript.KnockBackSpeed = KnockBackSpeed ;
            cannonScript.Trail = Trail;
            cannonScript.TrailLength = TrailLength;
            cannonScript.TrailColor = TrailColor;

        }

    }

    class CannonScript : Block.BlockScript
    {

        public CanonBlock CB;

        public BlockBehaviour BB;

        public AudioSource AS;

        public GameObject Bullet;

        /// <summary>
        /// 子弹刚体组件
        /// </summary>
        public Rigidbody BR;

        public TrailRenderer myTrailRenderer;

        public float Strength;

        public float BulletSpeed;

        public float KnockBackSpeed;

        public float RandomDelay;

        public float BulletMass;

        public float Drag;

        public float Interval;

        public bool InheritSize;

        public bool bBullet;

        public bool Trail;

        public float TrailLength;

        public Color TrailColor;

        float timer;

        private float knockBackSpeed;

        private void Start()
        {
            BB = GetComponent<BlockBehaviour>();
            AS = BB.GetComponent<AudioSource>();
            CB = BB.GetComponent<CanonBlock>();
            Bullet = CB.boltObject.gameObject;
            BR = Bullet.GetComponent<Rigidbody>();

            //BulletSpeed = (CB.boltSpeed * Strength) / 15f;
            //knockBackSpeed = KnockBackSpeed * (8000 + BulletMass * CB.boltSpeed * Strength / Time.fixedTime);

            //CB.enabled = !bBullet;
            timer = Interval;

            //if (bBullet)
            //{

            //    Bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    Bullet.SetActive(false);
            //    Bullet.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(transform.localScale.x, transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
            //    Bullet.name = "CannonBomb";
            //    Bullet.AddComponent<DestroyIfEditMode>();
            //    Bullet.GetComponent<Renderer>().material.color = Color.gray;
            //    BR = Bullet.AddComponent<Rigidbody>();
            //    BR.mass = BulletMass;
            //    BR.drag = BR.angularDrag = Drag;
            //    BR.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            //}
            //else
            //{
            //    CB.randomDelay = RandomDelay;
            //    CB.knockbackSpeed = knockBackSpeed;
            //}

            if (bBullet)
            {

                BR.mass = BulletMass;
                BR.drag = BR.angularDrag = Drag;

                Bullet.transform.localScale = !InheritSize ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.Scale(Vector3.one * Mathf.Min(transform.localScale.x, transform.localScale.z), new Vector3(0.5f, 0.5f, 0.5f));
            }

            if (Trail)
            {
                
                if (Bullet.GetComponent<TrailRenderer>() == null)
                {
                    myTrailRenderer = Bullet.AddComponent<TrailRenderer>();
                }
                else
                {
                    myTrailRenderer = Bullet.GetComponent<TrailRenderer>();
                    myTrailRenderer.enabled = Trail;
                }
                myTrailRenderer.autodestruct = false;
                myTrailRenderer.receiveShadows = false;
                myTrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                myTrailRenderer.startWidth = 0.5f;
                myTrailRenderer.endWidth = 0.1f;
                myTrailRenderer.time = TrailLength;

                myTrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
                myTrailRenderer.material.SetColor("_TintColor", TrailColor);
            }
            else
            {
                myTrailRenderer = Bullet.GetComponent<TrailRenderer>();
                if (myTrailRenderer)
                {
                    myTrailRenderer.enabled = Trail;
                }
            }

        }

        private void Update()
        {
            if (StatMaster.levelSimulating)
            {
                
                if (BB.KeyList.Find(match => match.Key == "shoot").IsDown && Interval > 0)
                {
                    if (timer > Interval)
                    {
                        timer = 0;
                        //if (bBullet)
                        //{
                        //    shoot();
                        //}
                        //else
                        //{
                            CB.Shoot();
                        //}
                    }
                    else
                    {
                        timer += Time.deltaTime;
                    }

                }
                else if (BB.KeyList.Find(match => match.Key == "shoot").IsReleased)
                {
                    timer = Interval;
                }

            }

        }


        //private void shoot()
        //{

        //    var bullet = (GameObject)Instantiate(Bullet, CB.boltSpawnPos.position, CB.boltSpawnPos.rotation);
        //    bullet.GetComponent<Rigidbody>().velocity = -transform.up * BulletSpeed;
        //    bullet.SetActive(true);

        //    gameObject.GetComponent<Rigidbody>().AddForce(knockBackSpeed * transform.up);

        //    CB.particles[0].Play();
        //    AS.Play();

        //}
    }

}
