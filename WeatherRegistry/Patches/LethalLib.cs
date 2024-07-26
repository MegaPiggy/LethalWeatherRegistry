using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LethalLib;
using LethalLib.Extras;
using MonoMod.RuntimeDetour;
using UnityEngine;
using static LethalLib.Modules.Weathers;

namespace WeatherRegistry.Patches
{
  public class LethalLibPatch
  {
    public static Dictionary<int, CustomWeather> GetLethalLibWeathers()
    {
      // Get all the weathers from LethalLib
      return LethalLib.Modules.Weathers.customWeathers;
    }

    public static List<Weather> ConvertLLWeathers()
    {
      Dictionary<int, CustomWeather> llWeathers = GetLethalLibWeathers();
      List<Weather> weathers = [];

      // list through all entries
      foreach (KeyValuePair<int, CustomWeather> LethalLibWeatherEntry in llWeathers)
      {
        CustomWeather llWeather = LethalLibWeatherEntry.Value;

        ImprovedWeatherEffect effect =
          new(llWeather.weatherEffect.effectObject, llWeather.weatherEffect.effectPermanentObject)
          {
            name = llWeather.name,
            SunAnimatorBool = llWeather.weatherEffect.sunAnimatorBool,
            DefaultVariable1 = llWeather.weatherVariable1,
            DefaultVariable2 = llWeather.weatherVariable2,
          };

        LevelWeatherType weatherType = EnumUtils.Create<LevelWeatherType>(LethalLibWeatherEntry.Key, llWeather.name);

        Weather weather =
          new(llWeather.name, effect)
          {
            VanillaWeatherType = weatherType,
            Origin = WeatherOrigin.LethalLib,
            Color = Defaults.LethalLibColor,
            DefaultWeight = 50,
          };
        weathers.Add(weather);

        WeatherManager.ModdedWeathers.Add(weatherType, weather);
        // Get key
      }

      return weathers;
    }

    public static void Init()
    {
      Plugin.logger.LogWarning("Disabling LethalLib injections");

      Plugin.harmony.Patch(
        AccessTools.Method(typeof(LethalLib.Modules.Weathers), "RegisterLevelWeathers_StartOfRound_Awake"),
        prefix: new HarmonyMethod(typeof(LethalLibPatch), nameof(StartOfRoundAwakePrefix))
      );

      Plugin.harmony.Patch(
        AccessTools.Method(typeof(LethalLib.Modules.Weathers), "TimeOfDay_Awake"),
        prefix: new HarmonyMethod(typeof(LethalLibPatch), nameof(TimeOfDayAwakePrefix))
      );
    }

    internal static bool StartOfRoundAwakePrefix(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
      Plugin.logger.LogWarning("Skipping LethalLib StartOfRound method");
      orig(self);
      return false;
    }

    internal static bool TimeOfDayAwakePrefix(On.TimeOfDay.orig_Awake orig, TimeOfDay self)
    {
      Plugin.logger.LogWarning("Skipping LethalLib TimeOfDay method");
      orig(self);
      return false;
    }
  }
}
