﻿using UnityEngine;
using System.Collections;
using Leap.Unity;

namespace Leap.Unity {
  public class WristLeapToIKBlend : HandTransitionBehavior {
    [Leap.Unity.Attributes.AutoFind]
    public Animator animator;
    public Chirality Handedness;
    private HandModel handModel;
    private Vector3 armDirection;
    public float ElbowOffset = -0.5f;

    private Vector3 startingPalmPosition;
    private Quaternion startingOrientation;
    private Transform palm;

    private Vector3 PalmPositionAtLateUpdate;
    private Quaternion PalmRotationAtLateUpdate;
    private float positionIKWeight;
    private float rotationIKWeight;

    private Transform Scapula;
    private Transform Shoulder;
    private Transform Elbow;
    public GameObject MarkerPrefab;
    public Transform ElbowMarker;
    public Transform ElbowIKTarget;
    private Transform Neck;
    private float shoulder_up_target_weight;
    private float shoulder_up_weight;
    private float shoulder_forward_weight;
    private float shoulder_forward_target_weight;


    protected override void Awake() {
      base.Awake();
      animator = transform.root.GetComponentInChildren<Animator>();
      handModel = transform.GetComponent<HandModel>();
      palm = GetComponent<HandModel>().palm;
      startingPalmPosition = palm.localPosition;
      startingOrientation = palm.localRotation;
      Neck = animator.GetBoneTransform(HumanBodyBones.Neck);
      if(Handedness == Chirality.Left){
        Scapula = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        Shoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Elbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
      }
      if (Handedness == Chirality.Right) {
        Scapula = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        Shoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Elbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
      }
      MarkerPrefab = Resources.Load("AxisTripod") as GameObject;
      if (ElbowMarker == null) {
        ElbowMarker = GameObject.Instantiate(MarkerPrefab).transform;
        ElbowMarker.transform.name = animator.transform.name + "_ElbowMarker" + Handedness.ToString();
        ElbowMarker.position = Elbow.position;
        ElbowMarker.rotation = Elbow.rotation;
      }
      if (ElbowIKTarget == null) {
        ElbowIKTarget = GameObject.Instantiate(MarkerPrefab).transform;
        ElbowIKTarget.transform.name = animator.transform.name + "_ElbowIKTarget" + Handedness.ToString();
      }
    }

    protected override void HandFinish() {
      StartCoroutine(LerpToStart());
      positionIKWeight = 0;
      rotationIKWeight = 0;
      shoulder_forward_target_weight = 0;
      shoulder_up_target_weight = 0;
    }
    protected override void HandReset() {
      StopAllCoroutines();
      positionIKWeight = 1;
      rotationIKWeight = 1;
    }
    void Update() {
      //get Arm Directions and set elbow target position
      armDirection = handModel.GetArmDirection();
      ElbowIKTarget.position = palm.position + (armDirection * ElbowOffset);
    }

    void LateUpdate() {
      PalmPositionAtLateUpdate = palm.position;
      PalmRotationAtLateUpdate = palm.rotation;
      ElbowMarker.position = Elbow.position;
      ElbowMarker.rotation = Elbow.rotation;
      shoulder_up_weight = Mathf.Lerp(shoulder_up_weight, shoulder_up_target_weight, .4f);
      shoulder_forward_weight = Mathf.Lerp(shoulder_forward_weight, shoulder_forward_target_weight, .4f);
    }

    public void OnAnimatorIK(int layerIndex) {
      //Debug.Log("IK");
      if (Handedness == Chirality.Left) {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, positionIKWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, PalmPositionAtLateUpdate);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, rotationIKWeight);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, PalmRotationAtLateUpdate);
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, positionIKWeight);
        animator.SetIKHintPosition(AvatarIKHint.LeftElbow, ElbowIKTarget.position);

        //Debug.Log(("ElbowMarker_L.position.y: " + ElbowMarker_L.position.y + " - Shoulder_L.position.y: " + Shoulder_L.position.y));
        if (ElbowMarker.position.y > Scapula.position.y) {
          shoulder_up_target_weight = (ElbowMarker.position.y - Shoulder.position.y) * 10f;
          animator.SetFloat("shoulder_up_left", shoulder_up_weight);
        }
        else {
          shoulder_up_target_weight = 0.0f;
          animator.SetFloat("shoulder_up_left", shoulder_up_weight);
        }
        if (ElbowMarker.position.z > Scapula.position.z) {
          //Debug.Log("Left Forward: " + shoulder_forward_target_weight);
          shoulder_forward_target_weight = (ElbowMarker.position.z - Shoulder.position.z) * 10f;
          animator.SetFloat("shoulder_forward_left", shoulder_forward_weight);
        }
        else {
          shoulder_forward_target_weight = 0.0f;
          animator.SetFloat("shoulder_forward_left", shoulder_forward_weight);
        }
      }
      if (Handedness == Chirality.Right) {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, positionIKWeight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, PalmPositionAtLateUpdate);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rotationIKWeight);
        animator.SetIKRotation(AvatarIKGoal.RightHand, PalmRotationAtLateUpdate);
        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, positionIKWeight);
        animator.SetIKHintPosition(AvatarIKHint.RightElbow, ElbowIKTarget.position);
        if (ElbowMarker.position.y > Scapula.position.y + 0f) {
          //.Log("Right Above");
          shoulder_up_target_weight = (ElbowMarker.position.y - Shoulder.position.y) * 10f;
          animator.SetFloat("shoulder_up_right", shoulder_up_weight);
        }
        else {
          shoulder_up_target_weight = 0.0f;
          animator.SetFloat("shoulder_up_right", shoulder_up_weight);
        }
        if (ElbowMarker.position.z > Scapula.position.z) {
          shoulder_forward_target_weight = (ElbowMarker.position.z - Shoulder.position.z) * 10f;
          //Debug.Log("Right Forward: " + shoulder_forward_target_weight);
          animator.SetFloat("shoulder_forward_right", shoulder_forward_weight);
        }
        else {
          shoulder_forward_target_weight = 0.0f;
          animator.SetFloat("shoulder_forward_right", shoulder_forward_weight);
        }
      }
    }
    private IEnumerator LerpPositionIKWeight(float destinationWeight, float duration) {
      return null;
    }


    private IEnumerator LerpToStart() {
      Vector3 droppedPosition = palm.localPosition;
      Quaternion droppedOrientation = palm.localRotation;
      float duration = .25f;
      float startTime = Time.time;
      float endTime = startTime + duration;

      while (Time.time <= endTime) {
        float t = (Time.time - startTime) / duration;
        palm.localPosition = Vector3.Lerp(droppedPosition, startingPalmPosition, t);
        palm.localRotation = Quaternion.Lerp(droppedOrientation, startingOrientation, t);
        yield return null;
      }
    }


    public override void OnSetup() {
      Awake();
      MarkerPrefab = Resources.Load("AxisTripod") as GameObject;
      Debug.Log("MarkerPrefab: " + MarkerPrefab);
      Handedness = GetComponent<IHandModel>().Handedness;
      if (Handedness == Chirality.Left) {
        Scapula = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
        Shoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Elbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
      }
      if (Handedness == Chirality.Right) {
        Scapula = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
        Shoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        Elbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
      }

      //if (ElbowMarker == null) {
      //  ElbowMarker = GameObject.Instantiate(MarkerPrefab).transform;
      //  ElbowMarker.transform.name = animator.transform.name + "_ElbowMarker" + Handedness.ToString();
      //  ElbowMarker.position = Elbow.position;
      //  ElbowMarker.rotation = Elbow.rotation;
      //}
      //if (ElbowIKTarget == null) {
      //  ElbowIKTarget = GameObject.Instantiate(MarkerPrefab).transform;
      //  ElbowIKTarget.transform.name = animator.transform.name + "_ElbowIKTarget" + Handedness.ToString();
      //}
    }
  }
}