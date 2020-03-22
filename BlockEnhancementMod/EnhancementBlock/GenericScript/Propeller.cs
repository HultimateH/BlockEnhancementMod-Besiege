﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{   
    class Propeller_GenericEnhanceScript : ChangeHardnessBlock
    {
        MKey SwitchKey;
        //MMenu HardnessMenu;
        MToggle EffectToggle;
        MToggle ToggleToggle;
        MToggle LiftIndicatorToggle;

        //int HardnessIndex = 1;
        //bool Effect = true,Toggle = true,LiftIndicator = false;

        public override void SafeAwake()
        {

            SwitchKey = /*BB.*/AddKey(LanguageManager.Instance.CurrentLanguage.Enabled, "Switch", KeyCode.O);
            //SwitchKey.KeysChanged += ChangedProperties;

            HardnessMenu = /*BB.*/AddMenu("Hardness", /*HardnessIndex*/1, LanguageManager.Instance.CurrentLanguage.WoodenHardness/*, false*/);
            //HardnessMenu.ValueChanged += (int value) => { HardnessIndex = value; ChangedProperties(); };

            EffectToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.EnabledOnAwake, "Effect", /*Effect*/true);
            //EffectToggle.Toggled += (bool value) => { Effect = value; ChangedProperties(); };

            ToggleToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.ToggleMode, "Toggle Mode", /*Toggle*/true);
            //ToggleToggle.Toggled += (value) => { Toggle = value; ChangedProperties(); };

            LiftIndicatorToggle = /*BB.*/AddToggle(LanguageManager.Instance.CurrentLanguage.LiftIndicator, "Lift Indicator", /*LiftIndicator*/false);
            //LiftIndicatorToggle.Toggled += (value) => { LiftIndicator = value; ChangedProperties(); };

            base.SafeAwake();

#if DEBUG
            ConsoleController.ShowMessage("桨叶添加进阶属性");
#endif
        } 
     
        //public override void DisplayInMapper(bool value)
        //{
        //    SwitchKey.DisplayInMapper = value;
        //    HardnessMenu.DisplayInMapper = value;
        //    EffectToggle.DisplayInMapper = value;
        //    ToggleToggle.DisplayInMapper = value;
        //    LiftIndicatorToggle.DisplayInMapper = value;
        //}

        //private ConfigurableJoint ConfigurableJoint;
        private LineRenderer LR;
        private AxialDrag AD;
        
        private Vector3 liftVector;
        private Vector3 axisDragOrgin;

        public override void OnSimulateStartClient()
        {
            if (EnhancementEnabled)
            {
                ConfigurableJoint = GetComponent<ConfigurableJoint>();
                AD = GetComponent<AxialDrag>();
                axisDragOrgin = AD.AxisDrag;

                SetVelocityCap(/*Effect*/EffectToggle.IsActive);

                hardness.SwitchWoodHardness(/*HardnessIndex*/HardnessMenu.Value, ConfigurableJoint);

                if (/*LiftIndicator*/LiftIndicatorToggle.IsActive)
                {
                    LR = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();

                    LR.useWorldSpace = true;
                    LR.SetVertexCount(2);
                    LR.material = new Material(Shader.Find("Particles/Additive"));
                    LR.SetColors(Color.red, Color.yellow);
                    LR.SetWidth(0.5f, 0.5f);
                    LR.enabled = true;
                }
                else
                {
                    if (LR != null) Destroy(LR);
                }
            }        
        }
        public override void SimulateUpdateAlways_EnhancementEnable()
        {
            if (StatMaster.isClient) return;

            if (SwitchKey.IsPressed || SwitchKey.EmulationPressed())
            {
                //Effect = !Effect;
                EffectToggle.IsActive = !EffectToggle.IsActive;
                SetVelocityCap(/*Effect*/EffectToggle.IsActive);
            }

            if (!/*Toggle*/ToggleToggle.IsActive)
            {
                if (SwitchKey.IsReleased || SwitchKey.EmulationReleased())
                {
                    //Effect = !Effect;
                    EffectToggle.IsActive = !EffectToggle.IsActive;
                    SetVelocityCap(/*Effect*/EffectToggle.IsActive);
                }
            }

            if (/*LiftIndicator*/LiftIndicatorToggle.IsActive)
            {
                ////模拟速度向量转换到升力模块的坐标
                //AD.xyz = Vector3.Scale(AD.Rigidbody.transform.InverseTransformDirection(SettingWindow.simulateVelocity_Vector), ad.AxisDrag);
                ////计算模拟速度向量模的平方
                //ad.currentVelocitySqr = Mathf.Min(SettingWindow.simulateVelocity_Vector.sqrMagnitude, GetComponent<BlockBehaviour>().GetBlockID() == (int)BlockType.Wing ? 100 : 900);
                if (ConfigurableJoint != null)
                {
                    liftVector = AD.Rigidbody.transform.TransformVector(AD.xyz * AD.currentVelocitySqr);
                    LR.SetPosition(0, transform.TransformPoint(AD.Rigidbody.centerOfMass));
                    LR.SetPosition(1, transform.TransformPoint(AD.Rigidbody.centerOfMass) + liftVector);
                }
                else
                {
                    LR.enabled = false;
                }
            }          
        }

        private void SetVelocityCap(bool value)
        {
            AD.AxisDrag = (value == false) ? Vector3.zero : axisDragOrgin;
        }
    }
}
