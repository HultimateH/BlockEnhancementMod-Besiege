﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{   
    class PropellerScript : EnhancementBlock
    {
        MKey SwitchKey;
        MMenu HardnessMenu;
        MToggle EffectToggle;
        MToggle ToggleToggle;
        MToggle LiftIndicatorToggle;

        int Hardness = 1;
        bool Effect = true,Toggle = true,LiftIndicator = false;

        public override void SafeAwake()
        {

            SwitchKey = BB.AddKey(LanguageManager.Instance.CurrentLanguage.enabled, "Switch", KeyCode.O);
            SwitchKey.KeysChanged += ChangedProperties;

            HardnessMenu = BB.AddMenu("Hardness", Hardness, LanguageManager.Instance.CurrentLanguage.WoodenHardness, false);
            HardnessMenu.ValueChanged += (int value) => { Hardness = value; ChangedProperties(); };

            EffectToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.enabledOnAwake, "Effect", Effect);
            EffectToggle.Toggled += (bool value) => { Effect = value; ChangedProperties(); };

            ToggleToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.toggleMode, "Toggle Mode", Toggle);
            ToggleToggle.Toggled += (value) => { Toggle = value; ChangedProperties(); };

            LiftIndicatorToggle = BB.AddToggle(LanguageManager.Instance.CurrentLanguage.liftIndicator, "Lift Indicator", LiftIndicator);
            LiftIndicatorToggle.Toggled += (value) => { LiftIndicator = value; ChangedProperties(); };

#if DEBUG
            ConsoleController.ShowMessage("桨叶添加进阶属性");
#endif
        }

        private Dictionary<int, Vector3> Dic_AxisDrag = new Dictionary<int, Vector3>
        {
            { (int)BlockType.Propeller,new Vector3(0,0.015f,0) },
            { (int)BlockType.SmallPropeller,new Vector3(0,0.015f,0) },
            { (int)BlockType.Unused3,new Vector3(0,0.015f,0)},
            { (int)BlockType.Wing , new Vector3(0,0.04f,0) },
            { (int)BlockType.WingPanel , new Vector3(0,0.02f,0) },
        };
     
        public override void DisplayInMapper(bool value)
        {
            SwitchKey.DisplayInMapper = value;
            HardnessMenu.DisplayInMapper = value;
            EffectToggle.DisplayInMapper = value;
            ToggleToggle.DisplayInMapper = value;
        }

        private ConfigurableJoint CJ;
        private LineRenderer LR;
        private AxialDrag AD;
        
        private int MyId;
        private Vector3 liftVector;

        public override void OnSimulateStart()
        {
            MyId = GetComponent<BlockVisualController>().ID;
            CJ = GetComponent<ConfigurableJoint>();
            AD = GetComponent<AxialDrag>();

            SetVelocityCap(Effect);
                
            SwitchWoodHardness(Hardness, CJ);

            
            if (LiftIndicator)
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
        public override void SimulateUpdateEnhancementEnableAlways()
        {

            if (SwitchKey.IsPressed)
            {
                Effect = !Effect;
                SetVelocityCap(Effect);
            }

            if (!Toggle)
            {
                if (SwitchKey.IsReleased)
                {
                    Effect = !Effect;
                    SetVelocityCap(Effect);
                }
            }


            if (LiftIndicator)
            {
                ////模拟速度向量转换到升力模块的坐标
                //AD.xyz = Vector3.Scale(AD.Rigidbody.transform.InverseTransformDirection(SettingWindow.simulateVelocity_Vector), ad.AxisDrag);
                ////计算模拟速度向量模的平方
                //ad.currentVelocitySqr = Mathf.Min(SettingWindow.simulateVelocity_Vector.sqrMagnitude, GetComponent<BlockBehaviour>().GetBlockID() == (int)BlockType.Wing ? 100 : 900);
                if (CJ != null)
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
            AD.AxisDrag = (value == false) ? Vector3.zero : Dic_AxisDrag[MyId];
        }
    }
}
