﻿using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rollbar;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class EddnTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        class MockStarService : StarSystemRepository
        {
            public StarSystem GetOrCreateStarSystem(string name, bool refreshIfOutdated = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetOrFetchStarSystem(string name, bool fetchIfMissing = true)
            {
                throw new NotImplementedException();
            }

            public StarSystem GetStarSystem(string name, bool refreshIfOutdated = true)
            {
                switch(name)
                {
                    case "Artemis":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Artemis";
                            result.systemAddress = 3107509474002;
                            result.x = 14.28125M;
                            result.y = -63.1875M;
                            result.z = -24.875M;
                            return result;
                        }
                    case "Diaguandri":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Diaguandri";
                            result.systemAddress = 670417429889;
                            result.x = -41.06250M;
                            result.y = -62.15625M;
                            result.z = -103.25000M;

                            Station station = new Station();
                            station.name = "Ray Gateway";
                            station.marketId = 3223343616;
                            result.stations.Add(station);

                            return result;
                        }
                    case "Sol":
                        {
                            StarSystem result = new StarSystem();
                            result.name = "Sol";
                            result.systemAddress = 10477373803;
                            result.x = 0.0M;
                            result.y = 0.0M;
                            result.z = 0.0M;
                            return result;
                        }

                    default:
                        break;
                }
                return null;
            }

            public void LeaveStarSystem(StarSystem starSystem)
            {
                throw new NotImplementedException();
            }

            public void SaveStarSystem(StarSystem starSystem)
            {
                throw new NotImplementedException();
            }
        }

        private EDDNResponder.EDDNResponder makeTestEDDNResponder()
        {
            EDDNResponder.EDDNResponder responder = new EDDNResponder.EDDNResponder(new MockStarService());
            return responder;
        }

        [TestMethod()]
        public void TestEDDNResponderGoodMatch()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", 10477373803);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool matched = responder.eventGetCoordinates("Sol", 10477373803);

            Assert.IsTrue(matched);
        }

        [TestMethod()]
        public void TestEDDNResponderBadInitialGoodFinal()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            // Intentionally place our EDDN responder in a state with no coordinates available.
            // The 'Docked' event does include systemName and systemAddress, so we set those here.
            privateObject.SetFieldOrProperty("systemName", "Artemis");
            privateObject.SetFieldOrProperty("systemAddress", 3107509474002);
            privateObject.SetFieldOrProperty("systemX", null);
            privateObject.SetFieldOrProperty("systemY", null);
            privateObject.SetFieldOrProperty("systemZ", null);

            bool matched = responder.eventGetCoordinates("Artemis", 3107509474002);

            Assert.IsTrue(matched);
            Assert.AreEqual("Artemis", responder.systemName);
            Assert.AreEqual(3107509474002, responder.systemAddress);
            Assert.AreEqual(14.28125M, (decimal)responder.systemX);
            Assert.AreEqual(-63.1875M, (decimal)responder.systemY);
            Assert.AreEqual(-24.875M, (decimal)responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderGoodInitialBadFinal()
        {
            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            var privateObject = new PrivateObject(responder);
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemAddress", 10477373803);
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            bool matched = responder.eventGetCoordinates("Not in this galaxy", null);

            Assert.IsFalse(matched);
            Assert.IsNull(responder.systemX);
            Assert.IsNull(responder.systemY);
            Assert.IsNull(responder.systemZ);
        }

        [TestMethod()]
        public void TestEDDNResponderDockedEvent()
        {
            string line = @"{
	""timestamp"": ""2018-07-30T06: 07: 47Z"",
	""event"": ""Docked"",
	""StationName"": ""Ray Gateway"",
	""StationType"": ""Coriolis"",
	""StarSystem"": ""Diaguandri"",
	""SystemAddress"": 670417429889,
	""MarketID"": 3223343616,
	""StationFaction"": ""EXO"",
	""FactionState"": ""Boom"",
	""StationGovernment"": ""$government_Democracy;"",
	""StationGovernment_Localised"": ""Democracy"",
	""StationServices"": [""Dock"",
	""Autodock"",
	""BlackMarket"",
	""Commodities"",
	""Contacts"",
	""Exploration"",
	""Missions"",
	""Outfitting"",
	""CrewLounge"",
	""Rearm"",
	""Refuel"",
	""Repair"",
	""Shipyard"",
	""Tuning"",
	""Workshop"",
	""MissionsGenerated"",
	""FlightController"",
	""StationOperations"",
	""Powerplay"",
	""SearchAndRescue"",
	""MaterialTrader"",
	""TechBroker""],
	""StationEconomy"": ""$economy_HighTech;"",
	""StationEconomy_Localised"": ""HighTech"",
	""StationEconomies"": [{
		""Name"": ""$economy_HighTech;"",
		""Name_Localised"": ""HighTech"",
		""Proportion"": 0.800000
	},
	{
		""Name"": ""$economy_Refinery;"",
		""Name_Localised"": ""Refinery"",
		""Proportion"": 0.200000
	}],
	""DistFromStarLS"": 566.487976
}";

            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            DockedEvent @event = (DockedEvent)events[0];

            EDDNResponder.EDDNResponder responder = makeTestEDDNResponder();
            responder.Handle(@event);

            // Test that data available from the event is set correctly
            Assert.AreEqual("Diaguandri", responder.systemName);
            Assert.AreEqual(670417429889, responder.systemAddress);
            Assert.AreEqual("Ray Gateway", responder.stationName);
            Assert.AreEqual(3223343616, responder.marketId);

            // Test metadata not in the event itself but retrieved from our local database
            Assert.AreEqual(-41.06250M, responder.systemX);
            Assert.AreEqual(-62.15625M, responder.systemY);
            Assert.AreEqual(-103.25000M, responder.systemZ);
        }
    }
}

