﻿using System;
using PKHeX.Core;
using System.Windows.Forms;
using System.Collections.Generic;
using PKHeX.Core.Searching;
using System.Linq;

namespace WangPlugin
{
    internal class SimpleEdit : Form
    {
        private IPKMView Editor { get; }
        private ISaveFileProvider SAV { get; }
        private Button AllRibbon;
        private Button LegalizeReport;
        private Button Master;
        private Button ClearRecord;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleEdit));
            this.AllRibbon = new System.Windows.Forms.Button();
            this.ClearRecord = new System.Windows.Forms.Button();
            this.LegalizeReport = new System.Windows.Forms.Button();
            this.Master = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AllRibbon
            // 
            this.AllRibbon.Location = new System.Drawing.Point(25, 26);
            this.AllRibbon.Name = "AllRibbon";
            this.AllRibbon.Size = new System.Drawing.Size(110, 30);
            this.AllRibbon.TabIndex = 0;
            this.AllRibbon.Text = "AllRibbon";
            this.AllRibbon.UseVisualStyleBackColor = true;
            this.AllRibbon.Click += new System.EventHandler(this.AllRibbon_Click);
            // 
            // ClearRecord
            // 
            this.ClearRecord.Location = new System.Drawing.Point(156, 26);
            this.ClearRecord.Name = "ClearRecord";
            this.ClearRecord.Size = new System.Drawing.Size(110, 30);
            this.ClearRecord.TabIndex = 1;
            this.ClearRecord.Text = "ClearRecord";
            this.ClearRecord.UseVisualStyleBackColor = true;
            this.ClearRecord.Click += new System.EventHandler(this.ClearRecord_Click);
            // 
            // LegalizeReport
            // 
            this.LegalizeReport.Location = new System.Drawing.Point(25, 71);
            this.LegalizeReport.Name = "LegalizeReport";
            this.LegalizeReport.Size = new System.Drawing.Size(110, 28);
            this.LegalizeReport.TabIndex = 2;
            this.LegalizeReport.Text = "Report";
            this.LegalizeReport.UseVisualStyleBackColor = true;
            this.LegalizeReport.Click += new System.EventHandler(this.LegalizeReport_Click);
            // 
            // Master
            // 
            this.Master.Location = new System.Drawing.Point(157, 71);
            this.Master.Name = "Master";
            this.Master.Size = new System.Drawing.Size(108, 28);
            this.Master.TabIndex = 3;
            this.Master.Text = "Master";
            this.Master.UseVisualStyleBackColor = true;
            this.Master.Click += new System.EventHandler(this.Master_Click);
            // 
            // SimpleEdit
            // 
            this.ClientSize = new System.Drawing.Size(287, 111);
            this.Controls.Add(this.Master);
            this.Controls.Add(this.LegalizeReport);
            this.Controls.Add(this.ClearRecord);
            this.Controls.Add(this.AllRibbon);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SimpleEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Super Wang";
            this.ResumeLayout(false);

        }
        public SimpleEdit(ISaveFileProvider sav, IPKMView editor)
        {
            Editor = editor;
            SAV = sav;
            InitializeComponent();
        }
        
        private void AllRibbon_Click(object sender, EventArgs e)
        {
            RibbonApplicator.SetAllValidRibbons(Editor.Data);
        }

        private void ClearRecord_Click(object sender, EventArgs e)
        {
            ITechRecord8 techRecord = Editor.Data as ITechRecord8;
            TechnicalRecordApplicator.SetRecordFlags(techRecord, false, 112);
        }

        private void LegalizeReport_Click(object sender, EventArgs e)
        {
            var la = new LegalityAnalysis(Editor.Data);
            MessageBox.Show($"{la.Report(true)}");
        }

        private void Master_Click(object sender, EventArgs e)
        {
            if (SAV.SAV.Version != GameVersion.PLA)
                MessageBox.Show("只适用于阿尔宙斯！");
            else
            {
                Handle_MoveShop();
                MessageBox.Show("搞定了！");
            }

        }
        private void Handle_MoveShop()
        {
            List<PKM> PokeList = new();
            var PokeArray = new PKM[30];
            var la = new LegalityAnalysis(Editor.Data);
            for (int i = 0; i < 30; i++)
            {
                PKM pkm = SAV.SAV.GetBoxSlotAtIndex(SAV.SAV.CurrentBox, i);
               var pko = pkm;
                var l = new LegalityAnalysis(pko);
                var pk = (PA8)pkm;
                if (pk.Species == 0)
                    break;
                pk = GetMoveFromDataBase(pkm);
                // MessageBox.Show($"{pk.AlphaMove}");
                IMoveShop8Mastery shop = pk;
                for (int q = 0; q < 62; q++)
                {
                    shop.SetPurchasedRecordFlag(q, true);
                    la = new LegalityAnalysis(pk);
                    if (!la.Valid)
                        shop.SetPurchasedRecordFlag(q, false);
                }
                for (int j = 0; j < 62; j++)
                {
                    shop.SetMasteredRecordFlag(j, true);
                    la = new LegalityAnalysis(pk);
                    if (!la.Valid)
                        shop.SetMasteredRecordFlag(j, false);
                }
                pk.Language = pkm.Language;
                pk.PID = pkm.PID;
                pk.EncryptionConstant = pkm.EncryptionConstant;
                pk.OT_Name = pkm.OT_Name;
                pk.TrainerID7 = pkm.TrainerID7;
                pk.TrainerSID7 = pkm.TrainerSID7;
                pk.OT_Gender = pkm.OT_Gender;
                pk.IVs = pkm.IVs;
                pk.EVs = pkm.EVs;
                pk.Nature = pkm.Nature;
                pk.AbilityNumber = pkm.AbilityNumber;
                pk.StatNature = pkm.StatNature;
                pk.CurrentLevel = pkm.CurrentLevel;
                pk.Ball = pkm.Ball;
                pk.GV_ATK = 7;
                pk.GV_DEF = 7;
                pk.GV_HP = 7;
                pk.GV_SPA = 7;
                pk.GV_SPD = 7;
                pk.GV_SPE = 7;
                pk.Form = pkm.Form;
                pk.RefreshChecksum();

                if (l.Valid)
                    PokeList.Add(pkm);
                else
                    PokeList.Add(pk);
            }
            var count = PokeList.Count;
            for (int i = 0; i < count; i++)
            {
                SAV.SAV.SetBoxSlotAtIndex(PokeList[i], SAV.SAV.CurrentBox, i);
            }
        }
        private PA8 GetMoveFromDataBase (PKM pk)
        {
            List<IEncounterInfo> Results = new();
            IEncounterInfo enc;
            PKM pkm = pk;
            var p = (PA8)pkm;
            var setting = new SearchSettings
            {
                Species = pk.Species,
                SearchEgg = false,
                Version = 47,
            };
            var search = EncounterUtil.SearchDatabase(setting, SAV.SAV);
            var results = search.ToList();
            if (results.Count != 0)
            {
                Results = results;
                enc = Results[0];
                var criteria = EncounterUtil.GetCriteria(enc, pkm);
                pkm = enc.ConvertToPKM(SAV.SAV, criteria);
                p = (PA8)pkm;
                for (int i = 0; i < results.Count; i++)
                {
                    if (p.IsAlpha == true&&p.Species== pk.Species&&p.Form==pk.Form)
                    {
                        p.SetShiny();
                        var la = new LegalityAnalysis(p);
                        if (la.Valid)
                            break;
                        else
                            continue;
                    }
                        enc = Results[i];
                        criteria = EncounterUtil.GetCriteria(enc, pk);
                        pkm = enc.ConvertToPKM(SAV.SAV, criteria);
                        p = (PA8)pkm;
                } 
            }
                return p;
        }
        
    }
}
