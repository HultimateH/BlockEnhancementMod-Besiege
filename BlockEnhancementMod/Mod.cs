﻿using Modding;
using UnityEngine;
using Modding.Levels;
using System.Collections.Generic;
using System;

namespace BlockEnhancementMod
{

    public class BlockEnhancementMod : ModEntryPoint
    {
        public static GameObject mod;
        public static Configuration Configuration { get; private set; }

        public override void OnLoad()
        {
            Configuration = Configuration.FormatXDataToConfig(/*Modding.Configuration.GetData()*/);

            mod = new GameObject("Block Enhancement Mod");
            UnityEngine.Object.DontDestroyOnLoad(mod);
            mod.AddComponent<EnhancementBlockController>();
            mod.AddComponent<ModSettingUI>();
            //mod.AddComponent<Zone>();

            //Controller.Instance.transform.SetParent(mod.transform);
            //ModSettingUI.Instance.transform.SetParent(mod.transform);
            LanguageManager.Instance.transform.SetParent(mod.transform);
            MessageController.Instance.transform.SetParent(mod.transform);
            RocketsController.Instance.transform.SetParent(mod.transform);

            //EnhancementEventsController events = mod.AddComponent<EnhancementEventsController>(); ;
            //ModEvents.RegisterCallback(1, events.OnGroup);
        }
    }

    public class Configuration
    {
        [Modding.Serialization.CanBeEmpty]
        public bool EnhanceMore = false;
        [Modding.Serialization.CanBeEmpty]
        public bool ShowUI = true;
        [Modding.Serialization.CanBeEmpty]
        public bool Friction = false;
        [Modding.Serialization.CanBeEmpty]
        public bool DisplayWaring = true;
        [Modding.Serialization.CanBeEmpty]
        public bool MarkTarget = true;
        [Modding.Serialization.CanBeEmpty]
        public bool DisplayRocketCount = true;

        [Modding.Serialization.CanBeEmpty]
        public float GuideControl_PFactor = 1.25f;
        [Modding.Serialization.CanBeEmpty]
        public float GuideControl_IFactor = 10f;
        [Modding.Serialization.CanBeEmpty]
        public float GuideControl_DFactor = 0f;


        public static Configuration FormatXDataToConfig(/*XDataHolder xDataHolder,*/Configuration config = null)
        {
            XDataHolder xDataHolder = Modding.Configuration.GetData();
            bool reWrite = true;

            if (config == null)
            {
                reWrite = false;
                config = new Configuration();
            }
   
            string[] keys = new string[] { "Enhance More", "ShowUI", "Friction", "Display Waring", "Mark Target", "Display Rocket Count", "PFactor", "IFactor", "DFactor" };

            config.EnhanceMore = getValue(keys[0], config.EnhanceMore);
            config.ShowUI = getValue(keys[1], config.ShowUI);
            config.Friction = getValue(keys[2], config.Friction);
            config.DisplayWaring = getValue(keys[3], config.DisplayWaring);
            config.MarkTarget = getValue(keys[4], config.MarkTarget);
            config.DisplayRocketCount = getValue(keys[5], config.DisplayRocketCount);

            config.GuideControl_PFactor = getValue(keys[6],config.GuideControl_PFactor);
            config.GuideControl_IFactor = getValue(keys[7],config.GuideControl_IFactor);
            config.GuideControl_DFactor = getValue(keys[8],config.GuideControl_DFactor);


            Modding.Configuration.Save();
            return config;

            T getValue<T> (string key ,T defaultValue)
            {
                foreach(var type in typeSpecialAction.Keys)
                {
                    if (defaultValue.GetType() == type)
                    {
                        if (xDataHolder.HasKey(key) && !reWrite)
                        {
                            defaultValue = (T)Convert.ChangeType(typeSpecialAction[type](xDataHolder, key), typeof(T));
                        }
                        else
                        {
                            xDataHolder.Write(key, defaultValue);
                        }
                        break;
                    }
                }
                return defaultValue;
            }
        }

        private static Dictionary<Type, Func<XDataHolder, string, object>> typeSpecialAction = new Dictionary<Type, Func<XDataHolder, string, object>>
        {
            { typeof(int), (xDataHolder,key)=>xDataHolder.ReadInt(key)},
            { typeof(bool), (xDataHolder,key)=>xDataHolder.ReadBool(key)},
            { typeof(float), (xDataHolder,key)=>xDataHolder.ReadFloat(key)},
            { typeof(string), (xDataHolder,key)=>xDataHolder.ReadString(key)},
            { typeof(Vector3), (xDataHolder,key)=>xDataHolder.ReadVector3(key)},
        };

        //public Configuration()
        //{
        //    EnhanceMore = false;
        //    ShowUI = true;
        //    Friction = false;
        //    DisplayWaring = true;
        //    MarkTarget = true;
        //    DisplayRocketCount = true;

        //    GuideControl_PFactor = 1.25f;
        //    GuideControl_IFactor = 10f;
        //    GuideControl_DFactor = 0f;
        //}
    }
}
