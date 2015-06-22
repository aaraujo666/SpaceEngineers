﻿
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Gui;
using Sandbox.Engine.Utils;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using System;
using System.Diagnostics;
using System.Text;
using VRage;
using VRage.Library.Utils;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;

namespace Sandbox.Game.Gui
{
    class MyGuiScreenEncounterSettings : MyGuiScreenBase
    { 
        private enum MyDamageEnum
        {
            NO_DAMAGE,
            ALMOST_NEW,
            LIGHT_DAMAGE,
            DAMAGED,
            HEAVILY_DAMAGED,
        }

        MyGuiScreenWorldSettings m_parent;
        bool m_isNewGame;
        bool m_isConfirmed;

        MyGuiControlButton m_okButton, m_cancelButton, m_peaceModeButton, m_adventureModeButton, m_warModeButton, m_ruinsModeButton, m_randomModeButton;
        MyGuiControlSlider m_maxNoShipsPerSpawnGroup, m_maxDamagedShipPercentage, m_maxHostileEncountersPercentage, m_antennaOnPercentage, m_reactorsOnPercentage;
        MyGuiControlCombobox m_maxDamagedShipsSeverity;
        MyGuiControlCheckbox m_antennaRangeMaxedOut, m_damageAppliedGlobally;

        public bool IsConfirmed
        {
            get
            {
                return m_isConfirmed;
            }
        }

        public MyGuiScreenEncounterSettings(MyGuiScreenWorldSettings parent)
            : base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, CalcSize(parent.Checkpoint))
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenEncounterSettings.ctor START");

            m_parent = parent;
            EnabledBackgroundFade = true;

            // TODO: May need to review this as I'm not sure I understand the checkpoint system well enough.
            m_isNewGame = (parent.Checkpoint == null);
            m_isConfirmed = false;

            RecreateControls(true);

            MySandboxGame.Log.WriteLine("MyGuiScreenEncounterSettings.ctor END");
        }

        public static Vector2 CalcSize(MyObjectBuilder_Checkpoint checkpoint)
        {
            float width = 0.9f;
            float height = 0.97f;

            return new Vector2(width, height);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            BuildControls();

            LoadValues();
        }

        public void BuildControls()
        {
            MyGuiControlParent parent = new MyGuiControlParent(size: new Vector2(Size.Value.X - 0.05f, Size.Value.Y));
            MyGuiControlScrollablePanel scrollPanel = new MyGuiControlScrollablePanel(parent);
            scrollPanel.ScrollbarVEnabled = false;
            scrollPanel.Size = new Vector2(Size.Value.X - 0.05f, 0.8f);

            Vector2 buttonSize = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 buttonsOrigin = m_size.Value / 2 - new Vector2(0.23f, 0.03f);

            AddCaption(MySpaceTexts.ScreenCaptionEncountersConfiguration);

            int numControls = 0;

            float severityComboBoxWidth = 0.2f;

            var maxNoShipsLabel = MakeLabel(MySpaceTexts.WorldSettings_MaxNoShipsPerSpawnGroup);
            var maxDamagedShipPercentageLabel = MakeLabel(MySpaceTexts.WorldSettings_DamagedEncounterLabel);
            var maxDamagedShipsSeverityLabel = MakeLabel(MySpaceTexts.WorldSettings_MaxDamagedShipsSeverity);
            var hostileEncountersLabel = MakeLabel(MySpaceTexts.WorldSettings_HostileEncountersLabel);
            var antennaActiveLabel = MakeLabel(MySpaceTexts.WorldSettings_AntennaActiveLabel);
            var antennaRangeMaxedLabel = MakeLabel(MySpaceTexts.WorldSettings_AntennaMaxedLabel);
            var reactorsOnPercentageLabel = MakeLabel(MySpaceTexts.WorldSettings_ReactorsOnLabel);
            var damageAppliedGloballyLabel = MakeLabel(MySpaceTexts.WorldSettings_DamageAppliedGloballyLabel);
            var presetLabel = MakeLabel(MySpaceTexts.WorldSettings_PresetValuesLabel);

            // Setup settings controls
            m_maxNoShipsPerSpawnGroup = new MyGuiControlSlider(
                position: Vector2.Zero - new Vector2(-0.1f, 0.4f),
                width: 0.2f,
                minValue: 1,
                maxValue: 5,
                labelText: new StringBuilder("{0}").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true,
                defaultValue: 2
                );

            m_maxNoShipsPerSpawnGroup.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsNoShips));

            m_maxHostileEncountersPercentage = new MyGuiControlSlider(
                position: Vector2.Zero - new Vector2(-0.1f, 0.4f),
                width: 0.2f,
                minValue: 0,
                maxValue: 100,
                labelText: new StringBuilder("{0}%").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true,
                defaultValue: 0
                );

            m_maxHostileEncountersPercentage.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsHostiles));           

            m_maxDamagedShipPercentage = new MyGuiControlSlider(
                position: Vector2.Zero - new Vector2(-0.1f, 0.3f),
                width: 0.2f,
                minValue: 0,
                maxValue: 100,
                labelText: new StringBuilder("{0}%").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true,
                defaultValue: 0
                );

            m_maxDamagedShipPercentage.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsDamagedShips));

            m_maxDamagedShipsSeverity = new MyGuiControlCombobox(size: new Vector2(severityComboBoxWidth, 0.04f));
            m_maxDamagedShipsSeverity.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsDamagedShipSeverity));

            m_maxDamagedShipsSeverity.AddItem((int)MyDamageEnum.NO_DAMAGE, MySpaceTexts.WorldSettings_NoDamage);
            m_maxDamagedShipsSeverity.AddItem((int)MyDamageEnum.ALMOST_NEW, MySpaceTexts.WorldSettings_Worn);
            m_maxDamagedShipsSeverity.AddItem((int)MyDamageEnum.LIGHT_DAMAGE, MySpaceTexts.WorldSettings_LightlyDamaged);
            m_maxDamagedShipsSeverity.AddItem((int)MyDamageEnum.DAMAGED, MySpaceTexts.WorldSettings_Damaged);
            m_maxDamagedShipsSeverity.AddItem((int)MyDamageEnum.HEAVILY_DAMAGED, MySpaceTexts.WorldSettings_HeavilyDamaged);

            m_damageAppliedGlobally = new MyGuiControlCheckbox();
            m_damageAppliedGlobally.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsDamageAppliedGlobally));

            m_reactorsOnPercentage = new MyGuiControlSlider(
                position: Vector2.Zero - new Vector2(-0.1f, 0.3f),
                width: 0.2f,
                minValue: 0,
                maxValue: 100,
                labelText: new StringBuilder("{0}%").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true,
                defaultValue: 0
                );

            m_reactorsOnPercentage.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsReactorsOn));

            m_antennaOnPercentage = new MyGuiControlSlider(
                position: Vector2.Zero - new Vector2(-0.1f, 0.3f),
                width: 0.2f,
                minValue: 0,
                maxValue: 100,
                labelText: new StringBuilder("{0}%").ToString(),
                labelDecimalPlaces: 0,
                labelSpaceWidth: 0.05f,
                intValue: true,
                defaultValue: 0
                );

            m_antennaOnPercentage.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsAntennasOn));

            m_antennaRangeMaxedOut = new MyGuiControlCheckbox();
            m_antennaRangeMaxedOut.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipEncounterSettingsAntennasRangeMaxed));                      
            
            // Ok-Cancel Buttons
            m_okButton = new MyGuiControlButton(position: buttonsOrigin - new Vector2(0.01f, 0f), size: buttonSize, text: MyTexts.Get(MySpaceTexts.Ok), onButtonClick: OkButtonClicked, originAlign: MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
            m_cancelButton = new MyGuiControlButton(position: buttonsOrigin + new Vector2(0.01f, 0f), size: buttonSize, text: MyTexts.Get(MySpaceTexts.Cancel), onButtonClick: CancelButtonClicked, originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
            
            // Themed Preset Buttons
            m_peaceModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_CURSOR_OVER, text: MyTexts.Get(MySpaceTexts.WorldSettings_PeaceMode), onButtonClick: PeaceButtonClicked);
            m_peaceModeButton.SetToolTip(MySpaceTexts.ToolTipEncounterSettings_PeaceMode);

            m_adventureModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_CURSOR_OVER, text: MyTexts.Get(MySpaceTexts.WorldSettings_AdventureMode), onButtonClick: AdventureButtonClicked);
            m_adventureModeButton.SetToolTip(MySpaceTexts.ToolTipEncounterSettings_AdventureMode);

            m_warModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_CURSOR_OVER, text: MyTexts.Get(MySpaceTexts.WorldSettings_WarMode), onButtonClick: WarButtonClicked);
            m_warModeButton.SetToolTip(MySpaceTexts.ToolTipEncounterSettings_WarMode);

            m_ruinsModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_CURSOR_OVER, text: MyTexts.Get(MySpaceTexts.WorldSettings_RuinsMode), onButtonClick: RuinsButtonClicked);
            m_ruinsModeButton.SetToolTip(MySpaceTexts.ToolTipEncounterSettings_RuinsMode);

            m_randomModeButton = new MyGuiControlButton(visualStyle: MyGuiControlButtonStyleEnum.Small, highlightType: MyGuiControlHighlightType.WHEN_CURSOR_OVER, text: MyTexts.Get(MySpaceTexts.WorldSettings_RandomMode), onButtonClick: RandomButtonClicked);
            m_randomModeButton.SetToolTip(MySpaceTexts.ToolTipEncounterSettings_RandomMode);

            float labelSize = 0.31f;

            float MARGIN_TOP = 0.15f;

            // Controls that will be automatically positioned
            parent.Controls.Add(maxNoShipsLabel);
            parent.Controls.Add(m_maxNoShipsPerSpawnGroup);

            parent.Controls.Add(hostileEncountersLabel);
            parent.Controls.Add(m_maxHostileEncountersPercentage);
            
            parent.Controls.Add(maxDamagedShipPercentageLabel);
            parent.Controls.Add(m_maxDamagedShipPercentage);

            parent.Controls.Add(maxDamagedShipsSeverityLabel);
            parent.Controls.Add(m_maxDamagedShipsSeverity);

            parent.Controls.Add(damageAppliedGloballyLabel);
            parent.Controls.Add(m_damageAppliedGlobally);

            parent.Controls.Add(reactorsOnPercentageLabel);
            parent.Controls.Add(m_reactorsOnPercentage);

            parent.Controls.Add(antennaActiveLabel);
            parent.Controls.Add(m_antennaOnPercentage);            

            parent.Controls.Add(antennaRangeMaxedLabel);
            parent.Controls.Add(m_antennaRangeMaxedOut);

            // Automatic layout - position all controls added up to this point.
            Vector2 originL, originC;
            Vector2 controlsDelta = new Vector2(0f, 0.052f);

            originL = -m_size.Value / 2 + new Vector2(0.16f, MARGIN_TOP);
            originC = originL + new Vector2(labelSize, 0f);
            float rightColumnOffset = originC.X + maxNoShipsLabel.Size.X - labelSize - 0.017f; 

            foreach (var control in parent.Controls)
            {
                control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                if (control is MyGuiControlLabel)
                    control.Position = originL + controlsDelta * numControls;
                else
                    control.Position = originC + controlsDelta * numControls++;
            }

            // The following controls need to be positioned manually.
            presetLabel.Position = originL + controlsDelta + new Vector2(0.23f, -0.09f);
            Controls.Add(presetLabel);

            m_peaceModeButton.Position = originL  + controlsDelta + new Vector2(0.02f, -0.03f);
            Controls.Add(m_peaceModeButton);

            m_adventureModeButton.Position = originL + controlsDelta + new Vector2(0.16f, -0.03f);
            Controls.Add(m_adventureModeButton);

            m_warModeButton.Position = originL + controlsDelta + new Vector2(0.30f, -0.03f);
            Controls.Add(m_warModeButton);

            m_ruinsModeButton.Position = originL + controlsDelta + new Vector2(0.44f, -0.03f);
            Controls.Add(m_ruinsModeButton);

            m_randomModeButton.Position = originL + controlsDelta + new Vector2(0.58f, -0.03f);
            Controls.Add(m_randomModeButton);

            Controls.Add(m_okButton);
            Controls.Add(m_cancelButton);

            Controls.Add(scrollPanel);
            CloseButtonEnabled = true;
        }

        private MyGuiControlLabel MakeLabel(MyStringId textEnum)
        {
            return new MyGuiControlLabel(text: MyTexts.GetString(textEnum), originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void LoadValues()
        {
            SetSettings(m_parent.Settings);
        }

        public void GetSettings(MyObjectBuilder_SessionSettings output)
        {
            output.MaxShipsInSpawnGroup = (short)m_maxNoShipsPerSpawnGroup.Value;
            output.MaxHostileEncountersPercentage = (int)m_maxHostileEncountersPercentage.Value;
            output.MaxDamagedShipsPercentage = (int)m_maxDamagedShipPercentage.Value;
            output.MaxDamagedShipsSeverity = (int)m_maxDamagedShipsSeverity.GetSelectedKey();
            output.DamageAppliedGlobally = (bool)m_damageAppliedGlobally.IsChecked;
            output.ReactorsOnPercentage = (int)m_reactorsOnPercentage.Value;
            output.AntennaOnPercentage = (int)m_antennaOnPercentage.Value;
            output.AntennaRangeMaxedOut = (bool)m_antennaRangeMaxedOut.IsChecked;          
        }

        public void SetSettings(MyObjectBuilder_SessionSettings settings)
        {
            m_maxNoShipsPerSpawnGroup.Value = settings.MaxShipsInSpawnGroup;
            m_maxHostileEncountersPercentage.Value = settings.MaxHostileEncountersPercentage;
            m_maxDamagedShipPercentage.Value = settings.MaxDamagedShipsPercentage;
            m_maxDamagedShipsSeverity.SelectItemByIndex(settings.MaxDamagedShipsSeverity);
            m_damageAppliedGlobally.IsChecked = settings.DamageAppliedGlobally;
            m_reactorsOnPercentage.Value = settings.ReactorsOnPercentage;
            m_antennaOnPercentage.Value = settings.AntennaOnPercentage;
            m_antennaRangeMaxedOut.IsChecked = settings.AntennaRangeMaxedOut;                        
        }

        public override string GetFriendlyName()
        {
            return "MyGuiScreenEncounterSettings";
        }

        private void PeaceButtonClicked(object sender)
        {
            m_maxNoShipsPerSpawnGroup.Value = 2;
            m_maxHostileEncountersPercentage.Value = 0;
            m_maxDamagedShipPercentage.Value = 0;
            m_maxDamagedShipsSeverity.SelectItemByIndex((int)MyDamageEnum.NO_DAMAGE);
            m_damageAppliedGlobally.IsChecked = false;
            m_reactorsOnPercentage.Value = 0;
            m_antennaOnPercentage.Value = 0;
            m_antennaRangeMaxedOut.IsChecked = false;
        }

        private void AdventureButtonClicked(object sender)
        {
            m_maxNoShipsPerSpawnGroup.Value = 3;
            m_maxHostileEncountersPercentage.Value = 50;
            m_maxDamagedShipPercentage.Value = 50;
            m_maxDamagedShipsSeverity.SelectItemByIndex((int)MyDamageEnum.DAMAGED);
            m_damageAppliedGlobally.IsChecked = false;
            m_reactorsOnPercentage.Value = 50;
            m_antennaOnPercentage.Value = 50;
            m_antennaRangeMaxedOut.IsChecked = false;
        }

        private void WarButtonClicked(object sender)
        {
            m_maxNoShipsPerSpawnGroup.Value = 5;
            m_maxHostileEncountersPercentage.Value = 100;
            m_maxDamagedShipPercentage.Value = 90;
            m_maxDamagedShipsSeverity.SelectItemByIndex((int)MyDamageEnum.HEAVILY_DAMAGED);
            m_damageAppliedGlobally.IsChecked = false;
            m_reactorsOnPercentage.Value = 100;
            m_antennaOnPercentage.Value = 100;
            m_antennaRangeMaxedOut.IsChecked = true;
        }

        private void RuinsButtonClicked(object sender)
        {
            m_maxNoShipsPerSpawnGroup.Value = 2;
            m_maxHostileEncountersPercentage.Value = 80;
            m_maxDamagedShipPercentage.Value = 100;
            m_maxDamagedShipsSeverity.SelectItemByIndex((int)MyDamageEnum.HEAVILY_DAMAGED);
            m_damageAppliedGlobally.IsChecked = true;
            m_reactorsOnPercentage.Value = 80;
            m_antennaOnPercentage.Value = 80;
            m_antennaRangeMaxedOut.IsChecked = false;            
        }

        private void RandomButtonClicked(object sender)
        {
            m_maxNoShipsPerSpawnGroup.Value = MyRandom.Instance.Next(1, 6); 
            m_maxHostileEncountersPercentage.Value = MyRandom.Instance.Next(0, 101);
            m_maxDamagedShipPercentage.Value = MyRandom.Instance.Next(0, 101);
            m_maxDamagedShipsSeverity.SelectItemByIndex(MyRandom.Instance.Next(0, 5));

            m_damageAppliedGlobally.IsChecked = MyRandom.Instance.Next(0, 2) == 1 ? true : false;
            m_reactorsOnPercentage.Value = MyRandom.Instance.Next(0, 101);
            m_antennaOnPercentage.Value = MyRandom.Instance.Next(0, 101);
            m_antennaRangeMaxedOut.IsChecked = MyRandom.Instance.Next(0, 2) == 1 ? true : false;

            OnOkButtonClicked();
            CloseScreen();
        }

        private void CancelButtonClicked(object sender)
        {
            this.CloseScreen();
        }

        private void OkButtonClicked(object sender)
        {
            m_isConfirmed = true;

            if (OnOkButtonClicked != null)
            {
                OnOkButtonClicked();
            }

            this.CloseScreen();
        }
        
        public event System.Action OnOkButtonClicked;
    }
}