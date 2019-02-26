﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using KSP;
using System.Text;
using KSPAchievements;
using MissionControllerEC.MCEParameters;
using KSP.Localization;

namespace MissionControllerEC.MCEContracts
{
    #region Build ComSat Network
    public class BuildComNetwork : Contract
    {
        CelestialBody targetBody = null;
        Settings settings = new Settings("Config.cfg");
        public double MinOrb;
        public double MaxOrb;
        public int crewCount = 0;
        public string ContractPlayerName;
        public int partAmount = 1;
        public string partName = "Repair Panel";
        public bool StartNetwork;
        public int totalContracts;
        public int TotalFinished;


        protected override bool Generate()
        {
            if (HighLogic.LoadedSceneIsFlight) { return false; }
            totalContracts = ContractSystem.Instance.GetCurrentContracts<BuildComNetwork>().Count();
            TotalFinished = ContractSystem.Instance.GetCompletedContracts<BuildComNetwork>().Count();
            bool parttechUnlock = ResearchAndDevelopment.GetTechnologyState("advConstruction") == RDTech.State.Available;
            if (totalContracts >= 1)
            {
                
                return false;
            }
            settings.Load();
            StartNetwork = SaveInfo.ComSateContractOn;
            if (!StartNetwork)
            {
                return false;
            }
            targetBody = FlightGlobals.Bodies[SaveInfo.comSatBodyName];
            ContractPlayerName = SaveInfo.ComSatContractName;
            MinOrb = SaveInfo.comSatminOrbital;
            MaxOrb = SaveInfo.comSatmaxOrbital;

            this.AddParameter(new OrbitalPeriod(targetBody, MinOrb, MaxOrb), null);
            if (parttechUnlock)
            {
                this.AddParameter(new PartGoal(partName, "Small Repair Panel",partAmount,true), null);
            }
            this.AddParameter(new GetCrewCount(crewCount), null);
            base.SetExpiry(3f, 15f);
            base.SetDeadlineYears(1f, targetBody);
            base.SetReputation(25f, 40f, targetBody);
            base.SetFunds(12000f * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 41000f * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 45000f * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, targetBody);

            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }
        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return targetBody.bodyName + MaxOrb + MinOrb + " - Total Done: " + TotalFinished + this.MissionSeed.ToString();
        }
        protected override string GetTitle()
        {
            return ContractPlayerName + " " + targetBody.bodyName;
        }
        protected override string GetDescription()
        {

            return Localizer.Format("#autoLOC_MissionController2_1000267");		// #autoLOC_MissionController2_1000267 = This is a ComSat Network contract, you have control over when these missions are available and what their orbital periods are, you can set this up in the MCE Infomation icon tab located top right \n corner of the screen while in Space Center View.  Please note that settings will not take effect until at least 1 cycle of contracts has passed.  If you don't see your settings cancel out the offered contract! \n\n All ComSat Information is stored inside the Mission Controller Config File and will pass on to other save files
        }
        protected override string GetNotes()
        {
            return Localizer.Format("#autoLOC_MissionController2_1000268");		// #autoLOC_MissionController2_1000268 = Vessel must be a new vessel launched after accepting this contract!
        }
        protected override string GetSynopsys()
        {
            return SaveInfo.SatelliteConDesc;
        }
        protected override string MessageCompleted()
        {
            SaveInfo.ComSateContractOn = false;
            return Localizer.Format("#autoLOC_MissionController2_1000270") + " " + targetBody.bodyName + " " + Localizer.Format("#autoLOC_MissionController2_1000271");		// #autoLOC_MissionController2_1000270 = You have delivered your ComSat to its assigned height around 		// #autoLOC_MissionController2_1000271 =  Continue to build you network.  When you're done you can turn off ComSat Contracts in the MCE Information Window.  Please note it will take a few contract cycles for them to disappear! 
        }

        protected override void OnLoad(ConfigNode node)
        {          
            Tools.ContractLoadCheck(node, ref targetBody, Planetarium.fetch.Home, targetBody, "targetBody");           
            Tools.ContractLoadCheck(node, ref MaxOrb, 71000, MaxOrb, "aPa");           
            Tools.ContractLoadCheck(node, ref MinOrb, 70500, MinOrb, "pEa");
            Tools.ContractLoadCheck(node, ref crewCount, 1, crewCount, "crewcount");
            Tools.ContractLoadCheck(node, ref partAmount, 1, partAmount, "partamount");
            Tools.ContractLoadCheck(node, ref partName, "Repair Panel", partName, "partname");
            Tools.ContractLoadCheck(node, ref ContractPlayerName, "Woops Defaults Loaded Error", ContractPlayerName, "contractplayername");
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            double ApAID = MaxOrb;
            node.AddValue("aPa", ApAID);
            double PeAID = MinOrb;
            node.AddValue("pEa", PeAID);
            node.AddValue("crewcount", crewCount);
            node.AddValue("partamount", partAmount);
            node.AddValue("partname", partName);
            node.AddValue("contractplayername", ContractPlayerName);
        }

        public override bool MeetRequirements()
        {
            bool techUnlock = ResearchAndDevelopment.GetTechnologyState("advFlightControl") == RDTech.State.Available;
            if (techUnlock)
                return true;
            else
                return false;
        }
    }
    #endregion
    #region Custom Supply Contract
    public class CustomSupply : Contract
    {
        Settings st = new Settings("Config.cfg");
        CelestialBody targetBody = null;
        public string vesselName;
        public string vesselId;
        public string ResourceName;
        public string ContractPlayerName;
        public bool StartSupply;
        public double resourcesAmount;
        public int totalContracts;
        public int TotalFinished;
        public string CTitle = Localizer.Format("#autoLOC_MCE_Supply_your_Station_Or_Base_with");
        ContractParameter suppy1;
        ContractParameter suppy2;
        

        protected override bool Generate()
        {
            if (HighLogic.LoadedSceneIsFlight) { return false; }
            totalContracts = ContractSystem.Instance.GetCurrentContracts<CustomSupply>().Count();
            TotalFinished = ContractSystem.Instance.GetCompletedContracts<CustomSupply>().Count();
            bool parttechUnlock = ResearchAndDevelopment.GetTechnologyState("advConstruction") == RDTech.State.Available;

            if (totalContracts >= 1)
            {
                return false;
            }
            StartSupply = SaveInfo.supplyContractOn;
            if (!StartSupply)
            {
                return false;
            }
            targetBody = FlightGlobals.Bodies[SaveInfo.SupplyBodyIDX];
            if (targetBody == null)
            {
                targetBody = Planetarium.fetch.Home;
            }
            vesselName = SaveInfo.SupplyVesName;
            vesselId = SaveInfo.SupplyVesId;
            ResourceName = SaveInfo.ResourceName;
            resourcesAmount = SaveInfo.supplyAmount;

            ContractPlayerName = SaveInfo.SupplyContractName;

            this.suppy1 = this.AddParameter(new TargetDockingGoal(vesselId, vesselName), null);
            suppy1.SetFunds(1000, 2000, targetBody);
            suppy1.SetReputation(5, 10, targetBody);
            this.suppy2 = this.AddParameter(new ResourceSupplyGoal(ResourceName, resourcesAmount, CTitle,true), null);
            suppy2.SetFunds(1000, 2000, targetBody);
            suppy2.SetReputation(5, 10, targetBody);
            base.SetExpiry(1f, 10f);
            base.SetDeadlineYears(.3f, targetBody);
            base.SetReputation(20f, 40f, targetBody);
            base.SetFunds(8000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 35000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 40000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, targetBody);

            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }
        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return Localizer.Format("#autoLOC_MissionController2_1000272") + " " + vesselName + " " + Localizer.Format("#autoLOC_MissionController2_1000273") + " " + targetBody + " " + Localizer.Format("#autoLOC_MissionController2_1000274") + TotalFinished + this.MissionSeed.ToString();		// #autoLOC_MissionController2_1000272 = Supply your base or station (		// #autoLOC_MissionController2_1000273 = ) with supplies. Location is 		// #autoLOC_MissionController2_1000274 =  - Total Done: 
        }
        protected override string GetTitle()
        {
            return ContractPlayerName + " (" + vesselName + ")";
        }
        protected override string GetDescription()
        {

            return Localizer.Format("#autoLOC_MCE_Custom_Supply_Contract_Desc") + " " + targetBody.name;
        }
        protected override string GetNotes()
        {
            return Localizer.Format("#autoLOC_MCE_Custom_Supply_Contract_GetNotes");
        }
        protected override string GetSynopsys()
        {
            return SaveInfo.ResourceTransferConDesc;
        }
        protected override string MessageCompleted()
        {
            SaveInfo.supplyContractOn = false;
            return Localizer.Format("#autoLOC_MCE_Custom_Supply_Contract_FinishedContract");
        }

        protected override void OnLoad(ConfigNode node)
        {          
            Tools.ContractLoadCheck(node, ref targetBody, Planetarium.fetch.Home, targetBody, "targetBody");
            Tools.ContractLoadCheck(node, ref vesselName, "defaults Loaded Error", vesselName, "vesselname");
            Tools.ContractLoadCheck(node, ref vesselId, "defaults", vesselId, "vesselid");
            Tools.ContractLoadCheck(node, ref ContractPlayerName, "Woops Defaults Loaded Error", ContractPlayerName, "contractplayername");
            Tools.ContractLoadCheck(node, ref ResourceName, "Woops Defaults Loaded Error", ResourceName, "supplies");
            Tools.ContractLoadCheck(node, ref resourcesAmount, 1, resourcesAmount, "resourceamount");
            Tools.ContractLoadCheck(node, ref CTitle, "Defaults Loaded Error", CTitle, "ctitle");
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            Debug.LogWarning("(supply) target body saved as " + bodyID);
            node.AddValue("targetBody", bodyID);
            node.AddValue("vesselname", vesselName);
            node.AddValue("vesselid", vesselId);
            node.AddValue("contractplayername", ContractPlayerName);
            node.AddValue("supplies", ResourceName);
            node.AddValue("resourceamount", resourcesAmount);
            node.AddValue("ctitle", CTitle);
        }

        public override bool MeetRequirements()
        {
            bool techUnlock = ResearchAndDevelopment.GetTechnologyState("advFlightControl") == RDTech.State.Available;
            if (techUnlock)
                return true;
            else
                return false;
        }
    }
    #endregion
    #region Custom Landing/Orbit Contract
    public class CustomLandingOrbit : Contract
    {
        CelestialBody targetBody = null;
        public string ContractPlayerName;
        public int crewAmount;
        public int totalContracts;
        public int TotalFinished;
        public string CTitle = Localizer.Format("#autoLOC_MCE2_Custom_Land_Orbit_Contract_Title_Set" + " ");
        public bool StartOrbitLand = false;
        ContractParameter Orbit1;
        ContractParameter Land2;
        ContractParameter crew1;

        protected override bool Generate()
        {
            if (HighLogic.LoadedSceneIsFlight) { return false; }
            totalContracts = ContractSystem.Instance.GetCurrentContracts<CustomLandingOrbit>().Count();
            TotalFinished = ContractSystem.Instance.GetCompletedContracts<CustomLandingOrbit>().Count();
            bool parttechUnlock = ResearchAndDevelopment.GetTechnologyState("advConstruction") == RDTech.State.Available;

            if (totalContracts >= 1)
            {
                return false;
            }
            StartOrbitLand = SaveInfo.OrbitLandingOn;
            if (!StartOrbitLand)
            {
                return false;
            }

            targetBody = FlightGlobals.Bodies[SaveInfo.LandingOrbitIDX];
            if (targetBody == null)
            {
                Debug.LogError("Could not find TargetBody for Custom Landing Orbit contract!!");
                return false;
            }

            ContractPlayerName = SaveInfo.LandingOrbitName;
            crewAmount = SaveInfo.LandingOrbitCrew;
            if (SaveInfo.IsOrbitOrLanding)
            {
                this.Orbit1 = this.AddParameter(new Contracts.Parameters.EnterOrbit(targetBody));
                Orbit1.SetFunds(2000 * crewAmount, 2000, targetBody);
                Orbit1.SetReputation(3 * crewAmount, targetBody);
            }

            else
            {
                this.Land2 = this.AddParameter(new Contracts.Parameters.LandOnBody(targetBody));
                Land2.SetFunds(1200 * crewAmount, targetBody);
                Land2.SetReputation(1 * crewAmount, targetBody);
            }
            crew1 = this.AddParameter(new GetCrewCount(crewAmount), null);
            crew1.SetFunds(1000 * crewAmount, targetBody);
            crew1.SetReputation(1 * crewAmount, targetBody);
            this.AddParameter(new Contracts.Parameters.KerbalDeaths(0));

            if (SaveInfo.OrbitAllowCivs == true)
            {
                SaveInfo.TourisNames.Clear();
                Tools.CivilianName();
                int test = 1;

                foreach (string name in SaveInfo.TourisNames)
                {

                    if (SaveInfo.IsOrbitOrLanding)
                    {
                        if (test <= SaveInfo.LandingOrbitCivilians)
                        {
                            FinePrint.Contracts.Parameters.KerbalTourParameter Kerbaltour = new FinePrint.Contracts.Parameters.KerbalTourParameter(name, ProtoCrewMember.Gender.Male);
                            this.AddParameter(Kerbaltour);
                            Kerbaltour.AddParameter(new FinePrint.Contracts.Parameters.KerbalDestinationParameter(targetBody, FlightLog.EntryType.Orbit, name));
                            Kerbaltour.SetFunds(Tools.FloatRandomNumber(7000, 17000), targetBody);
                            Kerbaltour.AddParameter(new LandOnBody(Planetarium.fetch.Home));
                            test++;
                        }
                    }
                    else
                    {
                        if (test <= SaveInfo.LandingOrbitCivilians)
                        {
                            FinePrint.Contracts.Parameters.KerbalTourParameter Kerbaltour = new FinePrint.Contracts.Parameters.KerbalTourParameter(name, ProtoCrewMember.Gender.Male);
                            this.AddParameter(Kerbaltour);
                            Kerbaltour.AddParameter(new FinePrint.Contracts.Parameters.KerbalDestinationParameter(targetBody, FlightLog.EntryType.Land, name));
                            Kerbaltour.SetFunds(Tools.FloatRandomNumber(7000, 17000), targetBody);
                            Kerbaltour.AddParameter(new LandOnBody(Planetarium.fetch.Home));
                            test++;
                        }
                    }
                }

            }

            base.SetExpiry(15f, 40f);
            base.SetDeadlineYears(700, targetBody);
            base.SetReputation(25f, 50f, targetBody);
            base.SetFunds(2500 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 45000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 55000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, targetBody);

            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }
        public override bool CanBeDeclined()
        {
            return true;
        }
        protected override void OnAccepted()
        {
            Tools.SpawnCivilianKerbMCE(SaveInfo.LandingOrbitCivilians);
        }
        protected override void OnCancelled()
        {
            Tools.CivilianGoHome();
        }

        protected override void OnFailed()
        {
            Tools.CivilianGoHome();
        }

        protected override void OnOfferExpired()
        {
            Tools.CivilianGoHome();
        }

        protected override string GetHashString()
        {
            return Localizer.Format("#autoLOC_MissionController2_1000265") + " " + crewAmount + " " + Localizer.Format("#autoLOC_MissionController2_1000266") + targetBody.bodyName + this.MissionSeed.ToString();		// #autoLOC_MissionController2_1000265 = Land Or Orbit 		// #autoLOC_MissionController2_1000266 =  Over 
        }
        protected override string GetTitle()
        {
            return ContractPlayerName;
        }
        protected override string GetDescription()
        {
            return SaveInfo.LandingOrbitDesc;
        }
        protected override string GetNotes()
        {
            return Localizer.Format("#autoLOC_MCE2_Custom_Land_Orbit_Contract_GetNotes_Set");
        }
        protected override string GetSynopsys()
        {
            return Localizer.Format("#autoLOC_MCE2_Custom_Land_Orbit_Contract_ContractDescription_Set");
        }
        protected override string MessageCompleted()
        {
            Tools.CivilianGoHome();
            SaveInfo.OrbitAllowCivs = false;
            SaveInfo.IsOrbitOrLanding = false;
            return Localizer.Format("#autoLOC_MCE2_Custom_Land_Orbit_Contract_ContractCompleted_Victory");
        }

        protected override void OnLoad(ConfigNode node)
        {
            Tools.ContractLoadCheck(node, ref targetBody, Planetarium.fetch.Home, targetBody, "targetBody");                  
            Tools.ContractLoadCheck(node, ref ContractPlayerName, "Woops Defaults Loaded Error", ContractPlayerName, "contractplayername");
            Tools.ContractLoadCheck(node, ref crewAmount, 1, crewAmount, "crew");            
            Tools.ContractLoadCheck(node, ref CTitle, "Defaults Loaded Error", CTitle, "ctitle");
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            Debug.LogWarning("Custom Land Orbit Saved as " + bodyID);
            node.AddValue("targetBody", bodyID); 
            node.AddValue("contractplayername", ContractPlayerName);
            node.AddValue("crew", crewAmount);
            node.AddValue("ctitle", CTitle);
        }

        //for testing purposes
        public override bool MeetRequirements()
        {
            bool techUnlock = ResearchAndDevelopment.GetTechnologyState("start") == RDTech.State.Available;
            if (techUnlock)
                return true;
            else
                return false;
        }
    }
    #endregion
    #region Custom Crew Transfer Contract
    public class CustomCrewTransfer : Contract
    {
        Settings st = new Settings("Config.cfg");
        CelestialBody targetBody = null;
        public string vesselName;
        public string vesselId;
        public bool Startcrewtrans;
        public int crewAmount;
        public string ContractPlayerName;
        public double crewTime;
        public int totalContracts;
        public int TotalFinished;
        public string CTitle = "Supply your Station Or Base with ";
        ContractParameter ctrans1;
        ContractParameter ctrans2;
        ContractParameter ctrans3;

        protected override bool Generate()
        {
            if (HighLogic.LoadedSceneIsFlight) { return false; }
            totalContracts = ContractSystem.Instance.GetCurrentContracts<CustomCrewTransfer>().Count();
            TotalFinished = ContractSystem.Instance.GetCompletedContracts<CustomCrewTransfer>().Count();
            bool parttechUnlock = ResearchAndDevelopment.GetTechnologyState("advConstruction") == RDTech.State.Available;

            if (totalContracts >= 1)
            {
                return false;
            }
            Startcrewtrans = SaveInfo.crewContractOn;
            if (!Startcrewtrans)
            {
                return false;
            }

            targetBody = FlightGlobals.Bodies[SaveInfo.crewBodyIDX];
            if (targetBody == null)
            {
                targetBody = Planetarium.fetch.Home;
            }
            vesselName = SaveInfo.crewVesName;
            vesselId = SaveInfo.crewVesid;
            crewAmount = SaveInfo.crewAmount;
            crewTime = SaveInfo.crewTime;
            double timemultiplier = Tools.ConvertDays(crewTime);

            ContractPlayerName = SaveInfo.crewTransferName;

            this.ctrans1 = this.AddParameter(new TargetDockingGoal(vesselId, vesselName), null);
            ctrans1.SetFunds(2000, 2000, targetBody);
            ctrans1.SetReputation(3, targetBody);
            this.ctrans2 = this.AddParameter(new TimeCountdownDocking(targetBody, crewTime, "Crew will Stay For This Amount Of Time ", vesselId, vesselName), null);
            ctrans2.SetFunds(Tools.RandomNumber(600,900) * (float)timemultiplier, targetBody);
            ctrans2.SetReputation(3, targetBody);
            this.AddParameter(new GetCrewCount(crewAmount), null);

            if (SaveInfo.transferTouristTrue == true)
            {
                SaveInfo.TourisNames2.Clear();
                Tools.CivilianName2();
                int test = 1;

                foreach (string name in SaveInfo.TourisNames2)
                {

                    if (test <= SaveInfo.transferTouristAmount)
                    {
                        FinePrint.Contracts.Parameters.KerbalTourParameter Kerbaltour2 = new FinePrint.Contracts.Parameters.KerbalTourParameter(name, ProtoCrewMember.Gender.Female);
                        this.AddParameter(Kerbaltour2);
                        Kerbaltour2.SetFunds(Tools.RandomNumber(800, 1900) * (float)timemultiplier, targetBody);
                        Kerbaltour2.AddParameter(new TargetDockingGoal(vesselId, vesselName));
                        Kerbaltour2.AddParameter(new LandOnBody(Planetarium.fetch.Home));                     
                        test++;
                    }

                }

            }

            this.ctrans3 = this.AddParameter(new LandingParameters(Planetarium.fetch.Home, true), null);
            ctrans3.SetFunds(2000, 2000, Planetarium.fetch.Home);
            ctrans3.SetReputation(3, Planetarium.fetch.Home);           
            base.SetExpiry(15f, 40f);
            base.SetDeadlineYears(700, targetBody);
            base.SetReputation(25f, 50f, targetBody);
            base.SetFunds(8000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 62000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, 72000 * HighLogic.CurrentGame.Parameters.CustomParams<MCE_IntergratedSettings3>().MCEContractPayoutMult, targetBody);

            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }
        public override bool CanBeDeclined()
        {
            return true;
        }
        protected override void OnAccepted()
        {
            Tools.SpawnCivilianKerbMCE2(SaveInfo.transferTouristAmount);
        }
        protected override void OnCancelled()
        {
            Tools.CivilianGoHome2();
        }

        protected override void OnFailed()
        {
            Tools.CivilianGoHome2();
        }

        protected override void OnOfferExpired()
        {
            Tools.CivilianGoHome2();
        }
        protected override void OnCompleted()
        {
            Tools.CivilianGoHome2();
        }
        protected override string GetHashString()
        {
            return "Transfer " + crewAmount + " Crew To (" + vesselName + ") for " + Tools.formatTime(crewTime) + " Over " + targetBody.bodyName + this.MissionSeed.ToString();
        }
        protected override string GetTitle()
        {
            return ContractPlayerName + " (" + vesselName + ")";
        }
        protected override string GetDescription()
        {

            return Localizer.Format("#autoLOC_MissionController2_1000275") + targetBody.bodyName + "." + " Payments are adjusted for Travel Time to Body";		// #autoLOC_MissionController2_1000275 = This is a custom Crew Transfer mission.  Use these contracts to supply your land bases and orbital stations with crew and select the Time Spent in station or base. You must dock with the Vessel you selected to finish contract! You can edit this contract by going to the Space Center screen and selecting the Mission Controller Icon.  In the GUI choose the Custom Contract Button to start editing this contract. \n\n All Crew Transfer contract information is stored in your Persistent Save File. The location of the Station or Base you will Transfer crew is 
        }
        protected override string GetNotes()
        {
            return Localizer.Format("#autoLOC_MissionController2_1000276");		// #autoLOC_MissionController2_1000276 = Vessel must be a new vessel launched after accepting this contract!
        }
        protected override string GetSynopsys()
        {
            return SaveInfo.TransferCrewDesc;
        }
        protected override string MessageCompleted()
        {
            Tools.CivilianGoHome2();
            SaveInfo.crewContractOn = false;
            return Localizer.Format("#autoLOC_MissionController2_1000278") + " " + vesselName + " " + Localizer.Format("#autoLOC_MissionController2_1000279") + " " + Tools.formatTime(crewTime) + " " + Localizer.Format("#autoLOC_MissionController2_1000280") +		// #autoLOC_MissionController2_1000278 = You have Delivered your Crew to 		// #autoLOC_MissionController2_1000279 = And spent 		// #autoLOC_MissionController2_1000280 =  at your Station/Base\n\n
                Localizer.Format("#autoLOC_MissionController2_1000281");		// #autoLOC_MissionController2_1000281 =  If you're done, you can turn off Crew Contracts in the MCE Information Window.  Please note it will take a few contract cycles for them to disappear! 
        }

        protected override void OnLoad(ConfigNode node)
        {
            Tools.ContractLoadCheck(node, ref targetBody, Planetarium.fetch.Home, targetBody, "targetBody");
            Tools.ContractLoadCheck(node, ref vesselName, "defaults Loaded Error", vesselName, "vesselname");
            Tools.ContractLoadCheck(node, ref vesselId, "defaults", vesselId, "vesselid");
            Tools.ContractLoadCheck(node, ref ContractPlayerName, "Woops Defaults Loaded Error", ContractPlayerName, "contractplayername");
            Tools.ContractLoadCheck(node, ref crewAmount, 1, crewAmount, "crew");
            Tools.ContractLoadCheck(node, ref crewTime, 10000, crewTime, "time");
            Tools.ContractLoadCheck(node, ref CTitle, "Defaults Loaded Error", CTitle, "ctitle");
        }
        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            Debug.LogWarning("(supply) target body saved as " + bodyID);
            node.AddValue("targetBody", bodyID);
            node.AddValue("vesselname", vesselName);
            node.AddValue("vesselid", vesselId);
            node.AddValue("contractplayername", ContractPlayerName);
            node.AddValue("crew", crewAmount);
            node.AddValue("time", crewTime);
            node.AddValue("ctitle", CTitle);
        }

        //for testing purposes
        public override bool MeetRequirements()
        {
            bool techUnlock = ResearchAndDevelopment.GetTechnologyState("advFlightControl") == RDTech.State.Available;
            if (techUnlock)
                return true;
            else
                return false;
        }
    }
    #endregion
}
