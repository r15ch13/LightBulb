﻿using System;
using LightBulb.Logic;
using LightBulb.Models;
using NUnit.Framework;

namespace LightBulb.Tests.Logic
{
    [TestFixture]
    public class FlowLogicTests
    {
        [Test]
        public void CalculateColorConfiguration_DayTime_Test()
        {
            // Arrange
            var sunriseTime = new TimeSpan(07, 00, 00);
            var sunsetTime = new TimeSpan(18, 00, 00);
            var transitionDuration = new TimeSpan(01, 30, 00);
            var dayConfiguration = new ColorConfiguration(6600, 1);
            var nightConfiguration = new ColorConfiguration(3600, 0.85);

            var instant = new DateTimeOffset(
                2019, 01, 01,
                14, 00, 00,
                TimeSpan.Zero);

            // Act
            var configuration = FlowLogic.CalculateColorConfiguration(
                sunriseTime, dayConfiguration,
                sunsetTime, nightConfiguration,
                transitionDuration, instant);

            // Assert
            Assert.That(configuration, Is.EqualTo(dayConfiguration));
        }

        [Test]
        public void CalculateColorConfiguration_NightTime_Test()
        {
            // Arrange
            var sunriseTime = new TimeSpan(07, 00, 00);
            var sunsetTime = new TimeSpan(18, 00, 00);
            var transitionDuration = new TimeSpan(01, 30, 00);
            var dayConfiguration = new ColorConfiguration(6600, 1);
            var nightConfiguration = new ColorConfiguration(3600, 0.85);

            var instant = new DateTimeOffset(
                2019, 01, 01,
                02, 00, 00,
                TimeSpan.Zero);

            // Act
            var configuration = FlowLogic.CalculateColorConfiguration(
                sunriseTime, dayConfiguration,
                sunsetTime, nightConfiguration,
                transitionDuration, instant);

            // Assert
            Assert.That(configuration, Is.EqualTo(nightConfiguration));
        }

        [Test]
        public void CalculateColorConfiguration_TransitionToNight_Test()
        {
            // Arrange
            var sunriseTime = new TimeSpan(07, 00, 00);
            var sunsetTime = new TimeSpan(18, 00, 00);
            var transitionDuration = new TimeSpan(01, 30, 00);
            var dayConfiguration = new ColorConfiguration(6600, 1);
            var nightConfiguration = new ColorConfiguration(3600, 0.85);

            var instant = new DateTimeOffset(
                2019, 01, 01,
                19, 00, 00,
                TimeSpan.Zero);

            // Act
            var configuration = FlowLogic.CalculateColorConfiguration(
                sunriseTime, dayConfiguration,
                sunsetTime, nightConfiguration,
                transitionDuration, instant);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.Temperature, Is.LessThan(dayConfiguration.Temperature), "Temperature < day temperature");
                Assert.That(configuration.Brightness, Is.LessThan(dayConfiguration.Brightness), "Brightness < day brightness");
                Assert.That(configuration.Temperature, Is.GreaterThan(nightConfiguration.Temperature), "Temperature > night temperature");
                Assert.That(configuration.Brightness, Is.GreaterThan(nightConfiguration.Brightness), "Brightness > night brightness");
            });
        }

        [Test]
        public void CalculateColorConfiguration_TransitionToDay_Test()
        {
            // Arrange
            var sunriseTime = new TimeSpan(07, 00, 00);
            var sunsetTime = new TimeSpan(18, 00, 00);
            var transitionDuration = new TimeSpan(01, 30, 00);
            var dayConfiguration = new ColorConfiguration(6600, 1);
            var nightConfiguration = new ColorConfiguration(3600, 0.85);

            var instant = new DateTimeOffset(
                2019, 01, 01,
                08, 00, 00,
                TimeSpan.Zero);

            // Act
            var configuration = FlowLogic.CalculateColorConfiguration(
                sunriseTime, dayConfiguration,
                sunsetTime, nightConfiguration,
                transitionDuration, instant);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(configuration.Temperature, Is.LessThan(dayConfiguration.Temperature), "Temperature < day temperature");
                Assert.That(configuration.Brightness, Is.LessThan(dayConfiguration.Brightness), "Brightness < day brightness");
                Assert.That(configuration.Temperature, Is.GreaterThan(nightConfiguration.Temperature), "Temperature > night temperature");
                Assert.That(configuration.Brightness, Is.GreaterThan(nightConfiguration.Brightness), "Brightness > night brightness");
            });
        }

        [Test(Description = "Ensures that the curve ends at the same value as it begins")]
        public void CalculateColorConfiguration_Circularity_Test()
        {
            // Arrange
            var sunriseTime = new TimeSpan(07, 00, 00);
            var sunsetTime = new TimeSpan(18, 00, 00);
            var transitionDuration = new TimeSpan(01, 30, 00);
            var dayConfiguration = new ColorConfiguration(6600, 1);
            var nightConfiguration = new ColorConfiguration(3600, 0.85);

            var instant1 = new DateTimeOffset(
                2019, 01, 01,
                00, 00, 00,
                TimeSpan.Zero);
            var instant2 = new DateTimeOffset(
                2019, 01, 01,
                23, 59, 59,
                TimeSpan.Zero);

            // Act
            var configuration1 = FlowLogic.CalculateColorConfiguration(
                sunriseTime, dayConfiguration,
                sunsetTime, nightConfiguration,
                transitionDuration, instant1);
            var configuration2 = FlowLogic.CalculateColorConfiguration(
                sunriseTime, dayConfiguration,
                sunsetTime, nightConfiguration,
                transitionDuration, instant2);

            // Assert
            Assert.That(configuration1, Is.EqualTo(configuration2));
        }

        [Test(Description = "Ensures that the curve is smooth")]
        public void CalculateColorConfiguration_Smoothness_Test()
        {
            // Arrange
            var sunriseTime = new TimeSpan(07, 00, 00);
            var sunsetTime = new TimeSpan(18, 00, 00);
            var transitionDuration = new TimeSpan(01, 30, 00);
            var dayConfiguration = new ColorConfiguration(6600, 1);
            var nightConfiguration = new ColorConfiguration(3600, 0.85);

            // Act
            var lastInstantTime = TimeSpan.Zero;
            var lastConfiguration = nightConfiguration;

            for (var instantTime = TimeSpan.Zero; instantTime < TimeSpan.FromDays(1); instantTime += TimeSpan.FromMinutes(1))
            {
                var instant = new DateTimeOffset(
                    2019, 01, 01,
                    instantTime.Hours, instantTime.Minutes, instantTime.Seconds,
                    TimeSpan.Zero);

                var configuration = FlowLogic.CalculateColorConfiguration(
                    sunriseTime, dayConfiguration,
                    sunsetTime, nightConfiguration,
                    transitionDuration, instant);

                // Assert
                if (Math.Abs(configuration.Temperature - lastConfiguration.Temperature) >=
                    Math.Abs(dayConfiguration.Temperature - nightConfiguration.Temperature) / 2)
                {
                    Assert.Fail($"Detected harsh jump in temperature between {instantTime} and {lastInstantTime}: " +
                                $"from {lastConfiguration} to {configuration}.");
                }

                if (Math.Abs(configuration.Brightness - lastConfiguration.Brightness) >=
                    Math.Abs(dayConfiguration.Brightness - nightConfiguration.Brightness) / 2)
                {
                    Assert.Fail($"Detected harsh jump in brightness between {instantTime} and {lastInstantTime}: " +
                                $"from {lastConfiguration} to {configuration}.");
                }

                lastInstantTime = instantTime;
                lastConfiguration = configuration;
            }
        }
    }
}