﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

namespace Leap.Unity.Interaction {

  public class ThrowingControllerSlidingWindow : IThrowingController {

    [Tooltip("How long the averaging window is in seconds.")]
    [SerializeField]
    private float _windowLength = 0.05f;

    [Tooltip("The delay between the averaging window and the current time.")]
    [SerializeField]
    private float _windowDelay = 0.02f;

    [SerializeField]
    private AnimationCurve _velocityScaleCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
                                                                    new Keyframe(3, 1.5f, 0, 0));

    private struct VelocitySample {
      public float time;
      public Vector3 position;
      public Quaternion rotation;

      public VelocitySample(Vector3 position, Quaternion rotation, float time) {
        this.position = position;
        this.rotation = rotation;
        this.time = Time.fixedTime;
      }

      public static VelocitySample Interpolate(VelocitySample a, VelocitySample b, float time) {
        float alpha = Mathf.Clamp01(Mathf.InverseLerp(a.time, b.time, time));
        return new VelocitySample(Vector3.Lerp(a.position, b.position, alpha),
                                  Quaternion.Slerp(a.rotation, b.rotation, alpha),
                                  time);
      }
    }

    private Queue<VelocitySample> _velocityQueue = new Queue<VelocitySample>();

    public override void OnHold(ReadonlyList<Hand> hands) {
      _velocityQueue.Enqueue(new VelocitySample(_obj.warper.RigidbodyPosition, _obj.warper.RigidbodyRotation, Time.fixedTime));

      while (true) {
        VelocitySample oldest = _velocityQueue.Peek();

        //Dequeue conservatively if the oldest is more than 4 frames later than the start of the window
        if (oldest.time + Time.fixedDeltaTime * 4 < Time.fixedTime - _windowLength - _windowDelay) {
          _velocityQueue.Dequeue();
        } else {
          break;
        }
      }
    }

    public override void OnThrow(Hand throwingHand) {
      float windowEnd = Time.fixedTime - _windowDelay;
      float windowStart = windowEnd - _windowLength;

      //0 occurs before 1
      //start occurs before end
      VelocitySample start0, start1;
      VelocitySample end0, end1;
      VelocitySample s0, s1;

      s0 = s1 = start0 = start1 = end0 = end1 = _velocityQueue.Dequeue();

      while (_velocityQueue.Count != 0) {
        s0 = s1;
        s1 = _velocityQueue.Dequeue();

        if (s0.time < windowStart && s1.time >= windowStart) {
          start0 = s0;
          start1 = s1;
        }

        if (s0.time < windowEnd && s1.time >= windowEnd) {
          end0 = s0;
          end1 = s1;

          //We have assigned both start and end and can break out of loop
          _velocityQueue.Clear();
          break;
        }
      }

      VelocitySample start = VelocitySample.Interpolate(start0, start1, windowStart);
      VelocitySample end = VelocitySample.Interpolate(end0, end1, windowEnd);

      _obj.rigidbody.velocity = PhysicsUtility.ToLinearVelocity(start.position, end.position, _windowLength);
      _obj.rigidbody.angularVelocity = PhysicsUtility.ToAngularVelocity(start.rotation, end.rotation, _windowLength);

      _obj.rigidbody.velocity *= _velocityScaleCurve.Evaluate(_obj.rigidbody.velocity.magnitude);
    }
  }
}
