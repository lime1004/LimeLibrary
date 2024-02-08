using System.Collections.Generic;
using System.Linq;
using LimeLibrary.Utility;
using UnityEngine;

namespace LimeLibrary.Module {

public class ObjectPool {
  private readonly GameObject _originalObject;
  private readonly Transform _parent;
  private readonly List<ObjectData> _poolList = new(256);

  private class ObjectData {
    public GameObject GameObject { get; }
    public bool IsAlloc { get; private set; }

    public ObjectData(GameObject gameObject) {
      GameObject = gameObject;
    }

    public void Alloc() {
      GameObject.SetActive(true);
      IsAlloc = true;
    }

    public void Release() {
      GameObject.SetActive(false);
      IsAlloc = false;
    }
  }

  public ObjectPool(GameObject originalObject, Transform parent = null) {
    _originalObject = originalObject;
    _parent = parent == null ? originalObject.transform.parent : parent;
    originalObject.SetActive(false);
  }

  public GameObject AllocObject() {
    _poolList.RemoveAll(data => data.GameObject == null);

    var freeObjectData = _poolList.FirstOrDefault(objectData => !objectData.IsAlloc);
    if (freeObjectData != null) {
      freeObjectData.Alloc();
      return freeObjectData.GameObject;
    } else {
      var createdObject = CreateObject();
      var newObjectData = new ObjectData(createdObject);
      newObjectData.Alloc();
      _poolList.Add(newObjectData);
      return createdObject;
    }
  }

  public void ReleaseObject(GameObject gameObject) {
    _poolList.RemoveAll(data => data.GameObject == null);

    var objectData = _poolList.FirstOrDefault(objectData => objectData.GameObject == gameObject);
    objectData?.Release();
  }

  public void ReleaseAll() {
    _poolList.RemoveAll(data => data.GameObject == null);

    foreach (var objectData in _poolList) {
      objectData.Release();
    }
  }

  private GameObject CreateObject() {
    return UnityUtility.Instantiate(_originalObject, _parent);
  }
}

}