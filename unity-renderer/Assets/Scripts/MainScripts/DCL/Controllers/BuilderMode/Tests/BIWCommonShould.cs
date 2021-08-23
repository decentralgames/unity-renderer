using Builder;
using Cinemachine;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWCommonShould : IntegrationTestSuite_Legacy
{
    private GameObject mockedGameObject;

    [Test]
    public void SettingsCorrectLayers()
    {
        //Arrange
        LayerMask SELECTION_LAYER_INDEX = LayerMask.NameToLayer("Selection");
        LayerMask DEFAULT_LAYER_INDEX = LayerMask.NameToLayer("Default");
        LayerMask COLLIDER_SELECTION_LAYER_INDEX = LayerMask.NameToLayer("OnBuilderPointerClick");
        LayerMask COLLIDER_SELECTION_LAYER = LayerMask.GetMask("OnBuilderPointerClick");
        LayerMask GIZMOS_LAYER = LayerMask.GetMask("Gizmo");
        LayerMask GROUND_LAYER = LayerMask.GetMask("Ground");

        //Assert
        Assert.AreEqual(SELECTION_LAYER_INDEX, BIWSettings.SELECTION_LAYER_INDEX);
        Assert.AreEqual(DEFAULT_LAYER_INDEX, BIWSettings.DEFAULT_LAYER_INDEX);
        Assert.AreEqual(COLLIDER_SELECTION_LAYER_INDEX, BIWSettings.COLLIDER_SELECTION_LAYER_INDEX);
        Assert.AreEqual(COLLIDER_SELECTION_LAYER, BIWSettings.COLLIDER_SELECTION_LAYER);
        Assert.AreEqual(GIZMOS_LAYER, BIWSettings.GIZMOS_LAYER);
        Assert.AreEqual(GROUND_LAYER, BIWSettings.GROUND_LAYER);
    }

    [Test]
    public void GroundRaycast()
    {
        //Arrange
        RaycastHit hit;

        Vector3 fromPosition = new Vector3(0, 10, 0);
        Vector3 toPosition = Vector3.zero;
        Vector3 direction = toPosition - fromPosition;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        //Act
        bool groundLayerFound = Physics.Raycast(fromPosition, direction, out hit, BIWGodMode.RAYCAST_MAX_DISTANCE, BIWSettings.GROUND_LAYER);
        if (Physics.Raycast(ray, out hit, BIWGodMode.RAYCAST_MAX_DISTANCE, BIWSettings.GROUND_LAYER))
        {
            groundLayerFound = true;
        }

        //Assert
        Assert.IsTrue(groundLayerFound, "The ground layer is not set to Ground");
    }

    [Test]
    public void BuilderInWorldEntityComponents()
    {
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        BIWEntity biwEntity = new BIWEntity();
        biwEntity.Init(scene.entities[entityId], null);

        Assert.IsTrue(biwEntity.entityUniqueId == scene.sceneData.id + scene.entities[entityId].entityId, "Entity id is not created correctly, this can lead to weird behaviour");

        SmartItemComponent.Model model = new SmartItemComponent.Model();

        scene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

        Assert.IsTrue(biwEntity.HasSmartItemComponent());

        DCLName name = (DCLName) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, name.id);

        DCLName dclName = biwEntity.rootEntity.TryGetComponent<DCLName>();
        Assert.IsNotNull(dclName);

        string newName = "TestingName";
        dclName.SetNewName(newName);
        Assert.AreEqual(newName, biwEntity.GetDescriptiveName());


        DCLLockedOnEdit entityLocked = (DCLLockedOnEdit) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, entityLocked.id);

        DCLLockedOnEdit dclLockedOnEdit = biwEntity.rootEntity.TryGetComponent<DCLLockedOnEdit>();
        Assert.IsNotNull(dclLockedOnEdit);

        bool isLocked = true;
        dclLockedOnEdit.SetIsLocked(isLocked);
        Assert.AreEqual(biwEntity.isLocked, isLocked);
    }

    [Test]
    public void GetCorrectSceneSize()
    {
        mockedGameObject = new GameObject("SceneSize");

        //Arrange
        var firstScene = CreateParcelSceneForSizeTest(new []
        {
            new Vector2Int(140, 140)
        });

        var secondScene = CreateParcelSceneForSizeTest(new []
        {
            new Vector2Int(140, 140),
            new Vector2Int(140, 141)
        });

        var thirdScene = CreateParcelSceneForSizeTest(new []
        {
            new Vector2Int(140, 140),
            new Vector2Int(141, 140)
        });

        var fourScene = CreateParcelSceneForSizeTest(new []
        {
            new Vector2Int(140, 140),
            new Vector2Int(141, 140),
            new Vector2Int(141, 141)

        });

        var fiveScene = CreateParcelSceneForSizeTest(new []
        {
            new Vector2Int(140, 140),
            new Vector2Int(139, 140),
            new Vector2Int(138, 140)

        });

        var sixScene = CreateParcelSceneForSizeTest(new []
        {
            new Vector2Int(140, 140),
            new Vector2Int(140, 139),
            new Vector2Int(140, 138)

        });

        //Act
        var firstResult = BIWUtils.GetSceneSize(firstScene);
        var secondResult = BIWUtils.GetSceneSize(secondScene);
        var thirdResult = BIWUtils.GetSceneSize(thirdScene);
        var fourResult = BIWUtils.GetSceneSize(fourScene);
        var fiveResult = BIWUtils.GetSceneSize(fiveScene);
        var sixResult = BIWUtils.GetSceneSize(sixScene);

        //Assert
        Assert.AreEqual(firstResult, new Vector2Int(1, 1));
        Assert.AreEqual(secondResult, new Vector2Int(1, 2));
        Assert.AreEqual(thirdResult, new Vector2Int(2, 1));
        Assert.AreEqual(fourResult, new Vector2Int(2, 2));
        Assert.AreEqual(fiveResult, new Vector2Int(3, 1));
        Assert.AreEqual(sixResult, new Vector2Int(1, 3));

    }

    private ParcelScene CreateParcelSceneForSizeTest(Vector2Int[] parcels)
    {
        ParcelScene scene = mockedGameObject.AddComponent<ParcelScene>();
        var data = new LoadParcelScenesMessage.UnityParcelScene();
        data.parcels = parcels;
        scene.SetData(data);
        return scene;
    }

    protected override IEnumerator TearDown()
    {
        if (mockedGameObject != null)
            GameObject.Destroy(mockedGameObject);
        AssetCatalogBridge.i.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        yield return base.TearDown();
    }
}