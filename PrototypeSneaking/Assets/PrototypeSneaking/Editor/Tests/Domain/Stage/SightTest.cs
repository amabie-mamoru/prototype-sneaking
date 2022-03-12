using System.Collections.Generic;
using NUnit.Framework;
using PrototypeSneaking.Domain.Stage;
using UnityEngine;

namespace PrototypeSneaking.Editor.Tests.Domain.Stage
{
    public class SightTest : TestBase
    {
        Sight sight;
        public SightTest()
        {
            var gameObj = new GameObject();
            sight = gameObj.AddComponent<Sight>();
        }

        [Test]
        public void Include()
        {
            var gameObj = new GameObject();
            var method = GetMethod<Sight>(sight, "Include");
            var field = GetField<Sight>(sight, "ObjectsInSight");

            // -----------------------------------
            // 検証対象以外は objectsInSight にカウントしない
            // -----------------------------------
            method.Invoke(sight, new object[] { gameObj });
            var objectsInSight = (List<GameObject>)field.GetValue(sight);
            Assert.AreEqual(0, objectsInSight.Count);


            // -----------------------------------
            // 検証対象だと objectsInSight としてカウント
            // -----------------------------------
            gameObj.AddComponent<Detectable>();
            method.Invoke(sight, new object[] { gameObj });

            objectsInSight = (List<GameObject>)field.GetValue(sight);
            Assert.AreEqual(1, objectsInSight.Count);
            Assert.AreEqual(true, objectsInSight.Contains(gameObj));

            // -----------------------------------
            // 同じオブジェクトを検出しても objectsInSight は増えない
            // -----------------------------------
            gameObj.AddComponent<Detectable>();
            method.Invoke(sight, new object[] { gameObj });

            objectsInSight = (List<GameObject>)field.GetValue(sight);
            Assert.AreEqual(1, objectsInSight.Count);
            Assert.AreEqual(true, objectsInSight.Contains(gameObj));

            // -----------------------------------
            // 別のオブジェクトを検出すると objectsInSight は増える
            // -----------------------------------
            var gameObj2 = new GameObject();
            gameObj2.AddComponent<Detectable>();
            method.Invoke(sight, new object[] { gameObj2 });

            objectsInSight = (List<GameObject>)field.GetValue(sight);
            Assert.AreEqual(2, objectsInSight.Count);
            Assert.AreEqual(true, objectsInSight.Contains(gameObj));
            Assert.AreEqual(true, objectsInSight.Contains(gameObj2));
        }

        [Test]
        public void Exclude()
        {
            var gameObj = new GameObject();
            var gameObj2 = new GameObject();
            gameObj.AddComponent<Detectable>();
            gameObj2.AddComponent<Detectable>();
            var gameObj3 = new GameObject();

            var fieldObjectsInSight = GetField<Sight>(sight, "ObjectsInSight");
            fieldObjectsInSight.SetValue(sight, new List<GameObject>() { gameObj, gameObj2 });

            var fieldFoundObjects = GetField<Sight>(sight, "FoundObjects");
            fieldFoundObjects.SetValue(sight, new List<GameObject>() { gameObj });

            var fieldLostCounter = GetField<Sight>(sight, "lostCounter");

            // -----------------------------------
            // 初期状態が正しいか確認
            // -----------------------------------
            var objectsInSight = (List<GameObject>)fieldObjectsInSight.GetValue(sight);
            var foundObjects = (List<GameObject>)fieldFoundObjects.GetValue(sight);
            var lostCounter = (uint)fieldLostCounter.GetValue(sight);
            Assert.AreEqual(2, objectsInSight.Count);
            Assert.AreEqual(true, objectsInSight.Contains(gameObj));
            Assert.AreEqual(true, objectsInSight.Contains(gameObj2));
            Assert.AreEqual(1, foundObjects.Count);
            Assert.AreEqual(true, foundObjects.Contains(gameObj));
            Assert.AreEqual(0, lostCounter);

            // -----------------------------------
            // Detectable でないオブジェクトはスルー
            // -----------------------------------
            var method = GetMethod<Sight>(sight, "Exclude");
            method.Invoke(sight, new object[] { gameObj3 });

            objectsInSight = (List<GameObject>)fieldObjectsInSight.GetValue(sight);
            foundObjects = (List<GameObject>)fieldFoundObjects.GetValue(sight);
            lostCounter = (uint)fieldLostCounter.GetValue(sight);
            Assert.AreEqual(2, objectsInSight.Count);
            Assert.AreEqual(1, foundObjects.Count);
            Assert.AreEqual(0, lostCounter);

            // -----------------------------------
            // Detectable だが objectsInSight にないものはスルー
            // -----------------------------------
            gameObj3.AddComponent<Detectable>();
            method.Invoke(sight, new object[] { gameObj3 });

            objectsInSight = (List<GameObject>)fieldObjectsInSight.GetValue(sight);
            foundObjects = (List<GameObject>)fieldFoundObjects.GetValue(sight);
            lostCounter = (uint)fieldLostCounter.GetValue(sight);
            Assert.AreEqual(2, objectsInSight.Count);
            Assert.AreEqual(1, foundObjects.Count);
            Assert.AreEqual(0, lostCounter);

            // -----------------------------------
            // objectsInSight にだけ含まれる場合は取り除き lostCounter を増やす
            // -----------------------------------
            method.Invoke(sight, new object[] { gameObj2 });

            objectsInSight = (List<GameObject>)fieldObjectsInSight.GetValue(sight);
            foundObjects = (List<GameObject>)fieldFoundObjects.GetValue(sight);
            lostCounter = (uint)fieldLostCounter.GetValue(sight);
            Assert.AreEqual(1, objectsInSight.Count);
            Assert.AreEqual(true, objectsInSight.Contains(gameObj));
            Assert.AreEqual(false, objectsInSight.Contains(gameObj2));
            Assert.AreEqual(1, foundObjects.Count);
            Assert.AreEqual(true, foundObjects.Contains(gameObj));
            Assert.AreEqual(0, lostCounter);


            // -----------------------------------
            // objectsInSight と foundObjects 両方に含まれる場合は両方から取り除き lostCounter を増やす
            // -----------------------------------
            method.Invoke(sight, new object[] { gameObj });

            objectsInSight = (List<GameObject>)fieldObjectsInSight.GetValue(sight);
            foundObjects = (List<GameObject>)fieldFoundObjects.GetValue(sight);
            lostCounter = (uint)fieldLostCounter.GetValue(sight);
            Assert.AreEqual(0, objectsInSight.Count);
            Assert.AreEqual(0, foundObjects.Count);
            Assert.AreEqual(1, lostCounter);
        }

        [Test]
        public void FindOrLoseWithRay()
        {
            var targetObj = new GameObject();
            var targetObj2 = new GameObject();
            var targetObj3 = new GameObject();
            var targetObj4 = new GameObject();
            var targetObj5 = new GameObject();
            var obstacle = new GameObject();
            var obstacle2 = new GameObject();
            // -----------------------------------
            // 位置関係
            //   obj1 <- obj2 <- obj3 <- obj4 <- obj5 <- obstacle <- obstacle2 <- sight
            // 初期化時は Collider を設置しないので Ray は貫通する
            // -----------------------------------
            targetObj2.transform.position = targetObj.transform.position + Vector3.back;
            targetObj3.transform.position = targetObj.transform.position + Vector3.back * 2;
            targetObj4.transform.position = targetObj.transform.position + Vector3.back * 3;
            targetObj5.transform.position = targetObj.transform.position + Vector3.back * 4;
            obstacle.transform.position = targetObj.transform.position + Vector3.back * 5;
            obstacle2.transform.position = targetObj.transform.position + Vector3.back * 6;
            sight.transform.position = targetObj.transform.position + Vector3.back * 7;

            targetObj.AddComponent<BoxCollider>();
          
            var fieldFoundCounter = GetField<Sight>(sight, "foundCounter");
            // -----------------------------------
            // 初期状態が正しいか確認
            // -----------------------------------
            var foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(0, sight.FoundObjects.Count);
            Assert.AreEqual(0, foundCounter);

            // -----------------------------------
            // edges がないケース
            // -----------------------------------
            // -----------------------------------
            // (1) 障害物がなく、対象レイヤーで、Detectable でない場合 foundObjects は変わらない
            // -----------------------------------
            var eyePos = sight.transform.position;

            var method = GetOverloadMethod<Sight>(sight, "FindOrLoseWithRay", new System.Type[] { typeof(Vector3), typeof(GameObject) });
            method.Invoke(sight, new object[] { eyePos, targetObj });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(0, sight.FoundObjects.Count);
            Assert.AreEqual(0, foundCounter);

            // -----------------------------------
            // (2) 障害物がなく、無視すべきレイヤーのオブジェクトで、Detectable な場合 foundObjects は変わらない
            // -----------------------------------
            targetObj2.layer = 2;
            targetObj2.AddComponent<BoxCollider>();
            targetObj2.AddComponent<Detectable>();
            method.Invoke(sight, new object[] { eyePos, targetObj });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(0, sight.FoundObjects.Count);
            Assert.AreEqual(0, foundCounter);

            // -----------------------------------
            // (3) 障害物がなく、対象レイヤーで、Detectable な場合 foundObjects が減り foundCounter が増える
            // -----------------------------------
            targetObj2.layer = 0;
            method.Invoke(sight, new object[] { eyePos, targetObj2 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(1, sight.FoundObjects.Count);
            Assert.AreEqual(true, sight.FoundObjects.Contains(targetObj2));
            Assert.AreEqual(1, foundCounter);

            // -----------------------------------
            // (4) すでに発見済みのオブジェクトに対しては foundObjects は変わらない
            // -----------------------------------
            method.Invoke(sight, new object[] { eyePos, targetObj2 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(1, sight.FoundObjects.Count);
            Assert.AreEqual(true, sight.FoundObjects.Contains(targetObj2));
            Assert.AreEqual(1, foundCounter);

            // -----------------------------------
            // (5) 障害物があり、Detectable なオブジェクトが見つからない場合 foundObjects は変わらない
            // -----------------------------------
            targetObj3.AddComponent<BoxCollider>();
            targetObj3.AddComponent<Detectable>();

            obstacle.AddComponent<BoxCollider>();

            method.Invoke(sight, new object[] { eyePos, targetObj3 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(1, sight.FoundObjects.Count);
            Assert.AreEqual(false, sight.FoundObjects.Contains(targetObj3));
            Assert.AreEqual(1, foundCounter);

            // -----------------------------------
            // (6) 障害物があったとしても、その障害物のレイヤーが無視レイヤーなら foundObjects が減り foundCounter が増える
            // -----------------------------------
            obstacle.layer = 2;
            method.Invoke(sight, new object[] { eyePos, targetObj3 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(2, sight.FoundObjects.Count);
            Assert.AreEqual(true, sight.FoundObjects.Contains(targetObj3));
            Assert.AreEqual(2, foundCounter);

            // -----------------------------------
            // edges があるケース
            // Character を継承する都合、Detectable でないケースは除外してテストする
            // -----------------------------------
            obstacle.SetActive(false);

            // -----------------------------------
            // (2') 障害物がなく、無視すべきレイヤーのオブジェクトで、Detectable な場合 foundObjects は変わらない
            // -----------------------------------
            targetObj4.layer = 2;
            targetObj4.AddComponent<BoxCollider>();
            targetObj4.AddComponent<Detectable>();

            method = GetOverloadMethod<Sight>(sight, "FindOrLoseWithRay", new System.Type[] { typeof(Vector3), typeof(Vector3), typeof(GameObject) });

            // edge は元のオブジェクトの場所に、オブジェクトは離れた位置にあっても動作することを保証する
            var edgePos = targetObj4.transform.position;
            targetObj4.transform.Translate(0, 100f, 0);
            method.Invoke(sight, new object[] { eyePos, edgePos, targetObj4 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(2, sight.FoundObjects.Count);
            Assert.AreEqual(2, foundCounter);

            // -----------------------------------
            // (3') 障害物がなく、対象レイヤーで、Detectable な場合 foundObjects が減り foundCounter が増える
            // -----------------------------------
            targetObj4.layer = 0;
            method.Invoke(sight, new object[] { eyePos, edgePos, targetObj4 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(3, sight.FoundObjects.Count);
            Assert.AreEqual(true, sight.FoundObjects.Contains(targetObj4));
            Assert.AreEqual(3, foundCounter);

            // -----------------------------------
            // (4') すでに発見済みのオブジェクトに対しては foundObjects は変わらない
            // -----------------------------------
            method.Invoke(sight, new object[] { eyePos, edgePos, targetObj4 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(3, sight.FoundObjects.Count);
            Assert.AreEqual(true, sight.FoundObjects.Contains(targetObj4));
            Assert.AreEqual(3, foundCounter);

            // -----------------------------------
            // (5') 障害物があり、Detectable なオブジェクトが見つからない場合 foundObjects は変わらない
            // -----------------------------------
            targetObj5.AddComponent<BoxCollider>();
            targetObj5.AddComponent<Detectable>();

            obstacle2.AddComponent<BoxCollider>();
            edgePos = targetObj5.transform.position;
            targetObj5.transform.Translate(0, -100f, 0);
            method.Invoke(sight, new object[] { eyePos, edgePos, targetObj5 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(3, sight.FoundObjects.Count);
            Assert.AreEqual(false, sight.FoundObjects.Contains(targetObj5));
            Assert.AreEqual(3, foundCounter);

            // -----------------------------------
            // (6) 障害物があったとしても、その障害物のレイヤーが無視レイヤーなら foundObjects が減り foundCounter が増える
            // -----------------------------------
            obstacle2.layer = 2;
            method.Invoke(sight, new object[] { eyePos, edgePos, targetObj5 });

            foundCounter = (uint)fieldFoundCounter.GetValue(sight);
            Assert.AreEqual(4, sight.FoundObjects.Count);
            Assert.AreEqual(true, sight.FoundObjects.Contains(targetObj5));
            Assert.AreEqual(4, foundCounter);
        }
    }
}