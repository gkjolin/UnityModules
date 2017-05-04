﻿using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Leap.Unity.Attributes;

namespace Leap.Unity.GraphicalRenderer {

  public interface ICustomChannelFeature {
    string channelName { get; }
  }

  [Serializable]
  public abstract class CustomChannelFeatureBase<T> : LeapGraphicFeature<T>, ICustomChannelFeature
    where T : LeapFeatureData, new() {

    [EditTimeOnly]
    [SerializeField]
    private string _channelName = "_CustomChannel";

    public string channelName {
      get {
        return _channelName;
      }
    }

    public override SupportInfo GetSupportInfo(LeapGraphicGroup group) {
      foreach (var feature in group.features) {
        if (feature == this) continue;

        var channelFeature = feature as ICustomChannelFeature;
        if (channelFeature != null && channelFeature.channelName == channelName) {
          return SupportInfo.Error("Cannot have two custom channels with the same name.");
        }
      }

      return SupportInfo.FullSupport();
    }
  }
}