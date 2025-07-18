using Microsoft.VisualStudio.TestTools.UnitTesting;
using ID.WeatherDashboard.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using ID.WeatherDashboard.API.Codes;
using ID.WeatherDashboard.APITests;
using System.Text.Json;
using ID.WeatherDashboard.APITests.Services;

namespace ID.WeatherDashboard.API.Services.Tests
{
    [TestClass]
    public class DataRetrieverServiceTests_Alert : DataRetrieverServiceTests
    {
        private Mock<IAlertQueryService> SetupAlertQueryService(string name)
        {
            var s = new Mock<IAlertQueryService>();
            s.SetupGet(m => m.ServiceName).Returns(name);
            AlertQueryServiceMocks.Add(s);
            return s;
        }

        private Alert GenerateAlert(Alert baseAlert, string? headline = null, 
            AlertMessageTypeEnum? messageType = null, AlertSeverityEnum? severity = null, 
            AlertUrgencyEnum? urgency = null, string? areas = null, 
            AlertCategoryEnum? category = null, AlertCertaintyEnum? certainty = null, 
            string? eventName = null, string? note = null, 
            DateTimeOffset? effective = null, DateTimeOffset? expires = null, 
            string? description = null, string? instruction = null)
        {
            return GenerateAlert(headline ?? baseAlert.Headline,
                messageType ?? baseAlert.MessageType,
                severity ?? baseAlert.Severity,
                urgency ?? baseAlert.Urgency,
                areas ?? baseAlert.Areas,
                category ?? baseAlert.Category,
                certainty ?? baseAlert.Certainty,
                eventName ?? baseAlert.Event,
                note ?? baseAlert.Note,
                effective ?? baseAlert.Effective,
                expires ?? baseAlert.Expires,
                description ?? baseAlert.Description,
                instruction ?? baseAlert.Instruction);
        }

        private Alert GenerateAlert(string? headline = null, AlertMessageTypeEnum? messageType = null, AlertSeverityEnum? severity = null, AlertUrgencyEnum? urgency = null, string? areas = null, AlertCategoryEnum? category = null, AlertCertaintyEnum? certainty = null, string? eventName = null, string? note = null, DateTimeOffset? effective = null, DateTimeOffset? expires = null, string? description = null, string? instruction = null)
        {
            return new Alert(DateTimeOffset.Now)
            {
                Category = category ?? TestHelpers.RandomEnumValue<AlertCategoryEnum>(),
                MessageType = messageType ?? AlertMessageTypeEnum.Alert,
                Headline = headline ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters),
                Areas = areas ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters),
                Certainty = certainty ?? TestHelpers.RandomEnumValue<AlertCertaintyEnum>(),
                Description = description ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters),
                Effective = effective ?? DateTimeOffset.Now.AddMinutes(-1),
                Event = eventName ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters),
                Expires = expires ?? DateTimeOffset.Now.AddHours(1),
                Instruction = instruction ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters),
                Urgency = urgency ?? TestHelpers.RandomEnumValue<AlertUrgencyEnum>(),
                Severity = severity ?? TestHelpers.RandomEnumValue<AlertSeverityEnum>(),
                Note = note ?? TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters)
            };
        }

        private void CheckDataUpdated(int currentData = 0, int forecastData = 0, int historyData = 0, int sunData = 0, int alertData = 0)
        {
            Assert.AreEqual(currentData, CurrentDataUpdated.Count);
            Assert.AreEqual(forecastData, ForecastDataUpdated.Count);
            Assert.AreEqual(historyData, HistoryDataUpdated.Count);
            Assert.AreEqual(sunData, SunDataUpdated.Count);
            Assert.AreEqual(alertData, AlertDataUpdated.Count);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldErrorIfNoServices()
        {
            var dr = GetDataRetriever();
            var l = new Location("Test");
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetAlertDataAsync(l), "No Invalid Operation Exception was thrown when running without services.");
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldErrorIfNoConfig()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetAlertDataAsync(l), "No Invalid Operation Exception was thrown when running without services.");
            alertService.Verify(a => a.GetAlertDataAsync(It.IsAny<Location>()), Times.Never());
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldErrorIfNoData()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetAlertDataAsync(l), "No Invalid Operation Exception was thrown when running without services.");
            alertService.Verify(a => a.GetAlertDataAsync(It.IsAny<Location>()), Times.Once());
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldReturnNothingIfAllDeletes()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(
                    new AlertData(DateTimeOffset.Now, new Alert(DateTimeOffset.Now)
                    {
                        Category = AlertCategoryEnum.Met,
                        MessageType = AlertMessageTypeEnum.Cancel
                    }
                ));
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            var d = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(d);
            Assert.IsFalse(d.Alerts.Any());
            CheckDataUpdated(alertData: 1);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldReturnAlert()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            var alert = GenerateAlert();
            var alertData = new AlertData(DateTimeOffset.Now, alert);
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            var d = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(d);
            var alerts = d.Alerts;
            Assert.AreEqual(1, alerts.Count());
            var alertCheck = alerts.First();
            Assert.AreEqual(alert.Headline, alertCheck.Headline);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldReturnNothingIfAllAlertsExpired()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            var alert = GenerateAlert(expires: DateTimeOffset.Now.AddSeconds(-1), effective: DateTimeOffset.Now.AddMinutes(-1));
            var alertData = new AlertData(DateTimeOffset.Now, alert);
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            var d = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(d);
            var alerts = d.Alerts;
            Assert.IsFalse(d.Alerts.Any());
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldHaveNoAlertsAfterUpdateToExpired()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            var alert = GenerateAlert();
            var updateAlert = GenerateAlert(alert, messageType: AlertMessageTypeEnum.Update, expires: DateTimeOffset.Now.AddSeconds(-1));
            var alertData = new AlertData(DateTimeOffset.Now, alert);
            var alertDataToSend = alertData;
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(() => alertDataToSend);
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            var d = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(d);
            var alerts = d.Alerts;
            Assert.IsTrue(d.Alerts.Any());
            d.Pulled = DateTimeOffset.Now.AddHours(-12);
            alertDataToSend = new AlertData(DateTimeOffset.Now, updateAlert);
            var newD = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(newD);
            var newAlerts = newD.Alerts;
            Assert.IsFalse(newAlerts.Any());
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldHaveNoAlertsAfterDelete()
        {
            var alertService = SetupAlertQueryService("AlertTest");
            var alert = GenerateAlert();
            var updateAlert = GenerateAlert(alert, messageType: AlertMessageTypeEnum.Cancel);
            var alertData = new AlertData(DateTimeOffset.Now, alert);
            var alertDataToSend = alertData;
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(() => alertDataToSend);
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            var d = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(d);
            var alerts = d.Alerts;
            Assert.IsTrue(d.Alerts.Any());
            d.Pulled = DateTimeOffset.Now.AddHours(-12);
            alertDataToSend = new AlertData(DateTimeOffset.Now, updateAlert);
            var newD = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(newD);
            var newAlerts = newD.Alerts;
            Assert.IsFalse(newAlerts.Any(), $"Alerts should have been empty, but instead they had: {JsonSerializer.Serialize(newD)}");
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldUpdateAlerts()
        {
            var generateNew = () => TestHelpers.RandomString(8, TestHelpers.UppercaseLetters, TestHelpers.LowercaseLetters);
            var alertService = SetupAlertQueryService("AlertTest");
            var alert = GenerateAlert();
            //Can't update Event, Areas, Category
            var updateAlert = GenerateAlert(alert, 
                messageType: AlertMessageTypeEnum.Update,
                headline: generateNew(),
                severity: TestHelpers.RandomEnumValue<AlertSeverityEnum>(),
                urgency: TestHelpers.RandomEnumValue<AlertUrgencyEnum>(),
                certainty: TestHelpers.RandomEnumValue<AlertCertaintyEnum>(),
                note: generateNew(),
                effective: DateTimeOffset.Now,
                expires: DateTime.Now.AddMinutes(25),
                description: generateNew(),
                instruction: generateNew()
                );
            var alertData = new AlertData(DateTimeOffset.Now, alert);
            var alertDataToSend = alertData;
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(() => alertDataToSend);
            Config.AlertData = GenerateAllStarConfig("AlertTest");
            var dr = GetDataRetriever();
            var l = new Location("Test");
            var d = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(d);
            var alerts = d.Alerts;
            Assert.IsTrue(d.Alerts.Any());
            d.Pulled = DateTimeOffset.Now.AddHours(-12);
            alertDataToSend = new AlertData(DateTimeOffset.Now, updateAlert);
            var newD = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(newD);
            var newAlerts = newD.Alerts;
            Assert.AreEqual(1, newAlerts.Count());
            var newAlert = newAlerts.First();
            Assert.AreEqual(updateAlert.Headline, newAlert.Headline);
            Assert.AreEqual(updateAlert.Severity, newAlert.Severity);
            Assert.AreEqual(updateAlert.Urgency, newAlert.Urgency);
            Assert.AreEqual(updateAlert.Certainty, newAlert.Certainty);
            Assert.AreEqual(updateAlert.Event, newAlert.Event);
            Assert.AreEqual(updateAlert.Note, newAlert.Note);
            Assert.AreEqual(updateAlert.Effective, newAlert.Effective);
            Assert.AreEqual(updateAlert.Expires, newAlert.Expires);
            Assert.AreEqual(updateAlert.Description, newAlert.Description);
            Assert.AreEqual(updateAlert.Instruction, newAlert.Instruction);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldMergeAlertsFromMultipleServices()
        {
            var alertService1 = SetupAlertQueryService("AlertService1");
            var alertService2 = SetupAlertQueryService("AlertService2");

            var alert1 = GenerateAlert(eventName: "StormWarning", areas: "Area1");
            var alert2 = GenerateAlert(eventName: "FloodAlert", areas: "Area2");

            var alertData1 = new AlertData(DateTimeOffset.Now, alert1);
            var alertData2 = new AlertData(DateTimeOffset.Now, alert2);

            alertService1.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData1);
            alertService2.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData2);

            Config.AlertData = GenerateAllStarConfig("AlertService1", "AlertService2");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetAlertDataAsync(l);

            Assert.IsNotNull(result);
            var alerts = result.Alerts.ToList();
            Assert.AreEqual(2, alerts.Count, "Expected two distinct alerts to be merged from both services.");

            Assert.IsTrue(alerts.Any(a => a.Event == alert1.Event && a.Areas == alert1.Areas), "Alert 1 was not present in merged alerts.");
            Assert.IsTrue(alerts.Any(a => a.Event == alert2.Event && a.Areas == alert2.Areas), "Alert 2 was not present in merged alerts.");

            CheckDataUpdated(alertData: 1);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldNotDuplicateSameAlertFromMultipleServices()
        {
            var alertService1 = SetupAlertQueryService("AlertService1");
            var alertService2 = SetupAlertQueryService("AlertService2");

            var commonEvent = "SevereThunderstorm";
            var commonAreas = "TestCounty";
            var commonCategory = AlertCategoryEnum.Met;

            var alertFromService1 = GenerateAlert(eventName: commonEvent, areas: commonAreas, category: commonCategory);
            var alertFromService2 = GenerateAlert(alertFromService1);

            var alertData1 = new AlertData(DateTimeOffset.Now, alertFromService1);
            var alertData2 = new AlertData(DateTimeOffset.Now, alertFromService2);

            alertService1.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData1);
            alertService2.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData2);

            Config.AlertData = GenerateAllStarConfig("AlertService1", "AlertService2");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetAlertDataAsync(l);

            Assert.IsNotNull(result);
            var alerts = result.Alerts.ToList();
            Assert.AreEqual(1, alerts.Count, $"Expected only one alert in result, duplicate alerts should not be added: {JsonSerializer.Serialize(result)}");

            var mergedAlert = alerts.First();
            Assert.AreEqual(commonEvent, mergedAlert.Event, "Alert Event mismatch.");
            Assert.AreEqual(commonAreas, mergedAlert.Areas, "Alert Areas mismatch.");
            Assert.AreEqual(commonCategory, mergedAlert.Category, "Alert Category mismatch.");

            CheckDataUpdated(alertData: 1);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldRetainAllAlertsWhenNoneExpired()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var alert1 = GenerateAlert(eventName: "HeatAdvisory", areas: "Region1", expires: DateTimeOffset.Now.AddHours(1));
            var alert2 = GenerateAlert(eventName: "AirQualityAlert", areas: "Region2", expires: DateTimeOffset.Now.AddHours(2));

            var alertData = new AlertData(DateTimeOffset.Now, alert1, alert2);

            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetAlertDataAsync(l);

            Assert.IsNotNull(result);

            var alerts = result.Alerts.ToList();

            Assert.AreEqual(2, alerts.Count, "Expected both non-expired alerts to remain in the result.");
            Assert.IsTrue(alerts.Any(a => a.Event == alert1.Event && a.Areas == alert1.Areas), "Alert 1 was missing.");
            Assert.IsTrue(alerts.Any(a => a.Event == alert2.Event && a.Areas == alert2.Areas), "Alert 2 was missing.");

            CheckDataUpdated(alertData: 1);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldClearOnlyExpiredAlerts()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var validAlert = GenerateAlert(eventName: "ValidWarning", areas: "Area1", expires: DateTimeOffset.Now.AddHours(1));
            var expiredAlert = GenerateAlert(eventName: "ExpiredWarning", areas: "Area2", expires: DateTimeOffset.Now.AddSeconds(-1));

            var alertData = new AlertData(DateTimeOffset.Now, validAlert, expiredAlert);

            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetAlertDataAsync(l);

            Assert.IsNotNull(result);

            var alerts = result.Alerts.ToList();

            Assert.AreEqual(1, alerts.Count, "Only one valid alert should remain after clearing expired alerts.");
            Assert.IsTrue(alerts.Any(a => a.Event == validAlert.Event && a.Areas == validAlert.Areas), "The valid alert was missing from the result.");
            Assert.IsFalse(alerts.Any(a => a.Event == expiredAlert.Event && a.Areas == expiredAlert.Areas), "The expired alert was not cleared as expected.");

            CheckDataUpdated(alertData: 1);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldUpdatePulledDateOnSuccess()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var alert = GenerateAlert(eventName: "TestEvent", areas: "TestArea", expires: DateTimeOffset.Now.AddHours(1));
            var initialAlertData = new AlertData(DateTimeOffset.Now.AddHours(-2), alert);
            var updatedAlertData = new AlertData(DateTimeOffset.Now, alert);

            var alertDataToReturn = initialAlertData;
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(() => alertDataToReturn);

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(firstResult);
            var firstPulled = firstResult.Pulled;

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            alertDataToReturn = updatedAlertData;

            var secondResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.IsTrue(secondResult.Pulled > firstPulled, $"Pulled date should have updated after fresh fetch. Instead, Pulled Date was {secondResult.Pulled} and the first pulled date was {firstPulled}");
            Assert.IsTrue(secondResult.Alerts.Any(), "Expected alerts to be present after update.");

            CheckDataUpdated(alertData: 2);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldReturnCachedDataIfNotExpired()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var alert = GenerateAlert(eventName: "CachedEvent", areas: "CachedArea", expires: DateTimeOffset.Now.AddHours(1));
            var initialAlertData = new AlertData(DateTimeOffset.Now, alert);

            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(initialAlertData);

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(firstResult);
            var firstPulled = firstResult.Pulled;

            var secondResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.AreSame(firstResult, secondResult, "Expected the cached data to be returned.");

            alertService.Verify(a => a.GetAlertDataAsync(It.IsAny<Location>()), Times.Once());

            CheckDataUpdated(alertData: 1);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldReplaceDataIfOverlayIsFalse()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var initialAlert = GenerateAlert(eventName: "InitialEvent", areas: "TestArea", expires: DateTimeOffset.Now.AddHours(1));
            var initialAlertData = new AlertData(DateTimeOffset.Now, initialAlert);

            var alertDataToReturn = initialAlertData;
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(() => alertDataToReturn);

            Config.AlertData = GenerateAllStarConfig("AlertService");
            Config.AlertData.OverlayExistingData = false;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            var refreshedAlert = GenerateAlert(eventName: "RefreshedEvent", areas: "TestArea", expires: DateTimeOffset.Now.AddHours(1));
            alertDataToReturn = new AlertData(DateTimeOffset.Now, refreshedAlert);

            var secondResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(secondResult);

            Assert.AreEqual(1, secondResult.Alerts.Count(), "Expected exactly one alert after replace.");
            Assert.AreEqual("RefreshedEvent", secondResult.Alerts.First().Event);

            alertService.Verify(a => a.GetAlertDataAsync(It.IsAny<Location>()), Times.Exactly(2));

            CheckDataUpdated(alertData: 2);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldOverlayDataWithNewOnRefreshIfOverlayIsTrue()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var initialAlert = GenerateAlert(eventName: "InitialEvent", areas: "TestArea", expires: DateTimeOffset.Now.AddHours(1));
            var initialAlertData = new AlertData(DateTimeOffset.Now, initialAlert);

            var alertDataToReturn = initialAlertData;
            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(() => alertDataToReturn);

            Config.AlertData = GenerateAllStarConfig("AlertService");
            Config.AlertData.OverlayExistingData = true;

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var firstResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(firstResult);

            firstResult.Pulled = DateTimeOffset.Now.AddHours(-12);

            var refreshedAlert = GenerateAlert(eventName: "RefreshedEvent", areas: "TestArea", expires: DateTimeOffset.Now.AddHours(1));
            alertDataToReturn = new AlertData(DateTimeOffset.Now, refreshedAlert);

            var secondResult = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(secondResult);

            var alerts = secondResult.Alerts.ToList();

            Assert.AreEqual(2, alerts.Count, "Expected both initial and refreshed alerts to be present due to overlay.");
            Assert.IsTrue(alerts.Any(a => a.Event == "InitialEvent"), "Initial alert should remain.");
            Assert.IsTrue(alerts.Any(a => a.Event == "RefreshedEvent"), "Refreshed alert should be added.");

            alertService.Verify(a => a.GetAlertDataAsync(It.IsAny<Location>()), Times.Exactly(2));

            CheckDataUpdated(alertData: 2);
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldRaiseAlertDataUpdatedEventOnSuccessfulRetrieval()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var alert = GenerateAlert(eventName: "EventForUpdate", areas: "TestArea", expires: DateTimeOffset.Now.AddHours(1));
            var alertData = new AlertData(DateTimeOffset.Now, alert);

            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetAlertDataAsync(l);
            Assert.IsNotNull(result);

            CheckDataUpdated(alertData: 1);
            Assert.AreEqual(l, AlertDataUpdated.First().Location, "AlertDataUpdated event fired with incorrect location.");
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldThrowWhenServiceThrowsException()
        {
            var alertService = SetupAlertQueryService("AlertService");

            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>()))
                .ThrowsAsync(new InvalidOperationException("Simulated service failure."));

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => dr.GetAlertDataAsync(l));

            Assert.AreEqual("Simulated service failure.", ex.Message, "Expected exception message did not match.");
            CheckDataUpdated();
        }

        [TestMethod]
        public async Task GetAlertDataAsync_ShouldNotThrowIfServiceReturnsNullAlerts()
        {
            var alertService = SetupAlertQueryService("AlertService");

            var alertData = new AlertData(DateTimeOffset.Now);

            alertService.Setup(a => a.GetAlertDataAsync(It.IsAny<Location>())).ReturnsAsync(alertData);

            Config.AlertData = GenerateAllStarConfig("AlertService");

            var dr = GetDataRetriever();
            var l = new Location("TestLocation");

            var result = await dr.GetAlertDataAsync(l);

            Assert.IsNotNull(result, "Expected AlertData to be returned, even with no alerts.");
            Assert.IsNotNull(result.Alerts, "Expected Alerts collection to be non-null.");
            Assert.IsFalse(result.Alerts.Any(), "Expected Alerts collection to be empty.");

            Assert.AreEqual(1, AlertDataUpdated.Count, "Expected AlertDataUpdated event to fire once even if no alerts.");
        }

    }
}