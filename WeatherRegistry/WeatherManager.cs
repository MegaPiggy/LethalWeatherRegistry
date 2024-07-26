using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace WeatherRegistry
{
  public static class WeatherManager
  {
    internal static bool IsSetupFinished = false;

    public static List<Weather> RegisteredWeathers { get; internal set; } = [];
    public static List<LevelWeather> LevelWeathers { get; internal set; } = [];

    public static Dictionary<LevelWeatherType, Weather> Weathers { get; internal set; } = [];
    public static Weather NoneWeather { get; internal set; }

    public static Dictionary<LevelWeatherType, Weather> ModdedWeathers = [];

    public static Dictionary<SelectableLevel, Weather> CurrentWeathers = [];

    public static void RegisterWeather(Weather weather)
    {
      RegisteredWeathers.Add(weather);
    }

    public static Weather GetWeather(LevelWeatherType levelWeatherType)
    {
      return Weathers[levelWeatherType];
    }

    public static void Reset()
    {
      IsSetupFinished = false;

      foreach (Weather weather in Weathers.Values)
      {
        if (weather.Origin != WeatherOrigin.WeatherRegistry)
        {
          GameObject.Destroy(weather.Effect);
          GameObject.Destroy(weather);
        }
      }

      foreach (LevelWeatherType weatherType in ModdedWeathers.Keys)
      {
        EnumUtils.Remove<LevelWeatherType>(weatherType);
      }

      // RegisteredWeathers.Clear();
      LevelWeathers.Clear();
      Weathers.Clear();
      ModdedWeathers.Clear();
      CurrentWeathers.Clear();

      Settings.ScreenMapColors.Clear();

      ConfigHelper.StringToWeather = null;

      RegisteredWeathers.RemoveAll(weather => weather.Origin != WeatherOrigin.WeatherRegistry);
    }

    // weathertweaks copy-paste:
    internal static List<LevelWeatherType> GetPlanetPossibleWeathers(SelectableLevel level)
    {
      List<LevelWeatherType> possibleWeathers = level
        .randomWeathers.Where(randomWeather => randomWeather.weatherType != LevelWeatherType.None)
        .Select(x => x.weatherType)
        .Distinct()
        .ToList();

      // add None as a possible weather in front of the list
      possibleWeathers.Insert(0, LevelWeatherType.None);

      Plugin.logger.LogDebug($"Possible weathers: {string.Join("; ", possibleWeathers.Select(x => x.ToString()))}");

      if (possibleWeathers == null || possibleWeathers.Count() == 0)
      {
        Plugin.logger.LogError("Level's random weathers are null");
        return [];
      }

      return possibleWeathers;
    }

    internal static MrovLib.WeightHandler<Weather> GetPlanetWeightedList(SelectableLevel level)
    {
      MrovLib.WeightHandler<Weather> weightedList = new();
      MrovLib.Logger logger = WeatherCalculation.Logger;

      List<LevelWeatherType> weatherTypes = GetPlanetPossibleWeathers(level);

      if (weatherTypes == null || weatherTypes.Count() == 0)
      {
        Plugin.logger.LogError("Level's random weathers are null");
        return weightedList;
      }

      foreach (var weather in weatherTypes)
      {
        // clone the object
        Weather typeOfWeather = GetWeather(weather);

        var weatherWeight = typeOfWeather.GetWeight(level);

        weightedList.Add(typeOfWeather, weatherWeight);
      }

      return weightedList;
    }

    internal static Weather GetCurrentWeather(SelectableLevel level)
    {
      if (CurrentWeathers.ContainsKey(level))
      {
        return CurrentWeathers[level];
      }
      else
      {
        return GetWeather(level.currentWeather);
      }
    }

    internal static string GetCurrentWeatherName(SelectableLevel level)
    {
      return GetCurrentWeather(level).Name;
    }

    internal static AnimationClip GetWeatherAnimationClip(LevelWeatherType weatherType)
    {
      return GetWeather(weatherType).AnimationClip;
    }
  }
}
