
#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class DetectiveSceneSetup
{
    [MenuItem("Detective/Setup Office Atmosphere")]
    public static void Setup()
    {
        GameObject Room = Selection.activeGameObject;

        if (Room == null)
        {
            EditorUtility.DisplayDialog(
                "Detective",
                "Hierarchy-ში ჯერ მონიშნე ოთახის მთავარი ობიექტი.",
                "OK"
            );
            return;
        }

        Renderer[] renderers =
            Room.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Detective",
                "მონიშნულ ობიექტში Renderer ვერ ვიპოვე.",
                "OK"
            );
            return;
        }

        // თუ წინა ვერსიით შექმნილი Atmosphere არსებობს,
        // წავშალოთ მხოლოდ ის და არა ოთახი ან ავეჯი.
        
        

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Setup Detective Office");

        // ოთახის საზღვრები
        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // ატმოსფეროს ობიექტი
        GameObject atmosphere = new GameObject("Atmosphere");
        Undo.RegisterCreatedObjectUndo(
            atmosphere,
            "Create Atmosphere"
        );

        // ----------------------------------------
        // 1. გარემოს განათება
        // ----------------------------------------

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight =
            new Color(0.32f, 0.31f, 0.30f);

        // ----------------------------------------
        // 2. ძირითადი Directional Light
        // ----------------------------------------

        Light[] lights = Object.FindObjectsByType<Light>(
            FindObjectsSortMode.None
        );

        foreach (Light lightSource in lights)
        {
            if (lightSource.type == LightType.Directional)
            {
                Undo.RecordObject(
                    lightSource,
                    "Adjust Office Directional Light"
                );

                lightSource.intensity = 1.0f;
                lightSource.color =
                    new Color(0.82f, 0.86f, 0.94f);
            }
        }

        // ----------------------------------------
        // 3. ოთახის რბილი, თბილი შუქი
        // ----------------------------------------

        CreateLight(
            atmosphere.transform,
            "Office Fill Light",
            LightType.Point,
            new Vector3(
                bounds.center.x,
                bounds.center.y + 1.0f,
                bounds.center.z
            ),
            new Color(1.0f, 0.88f, 0.74f),
            2.0f,
            Mathf.Max(bounds.size.magnitude, 6f),
            false
        );

        // ----------------------------------------
        // 4. ფანჯრის ცივი შუქი
        // იმუშავებს, თუ ფანჯრის ობიექტის სახელი
        // შეიცავს სიტყვას "window".
        // ----------------------------------------

        Transform window = FindByName(
            Room.transform,
            "window"
        );

        if (window != null)
        {
            Vector3 inward = bounds.center - window.position;
            inward.y = 0f;

            if (inward.sqrMagnitude < 0.001f)
            {
                inward = Vector3.forward;
            }
            else
            {
                inward.Normalize();
            }

            Vector3 windowLightPosition =
                window.position + inward * 0.8f;

            Light windowLight = CreateLight(
                atmosphere.transform,
                "Window Cool Light",
                LightType.Spot,
                windowLightPosition,
                new Color(0.60f, 0.72f, 0.95f),
                4.0f,
                8.0f,
                true
            );

            windowLight.spotAngle = 85f;

            Vector3 target = new Vector3(
                bounds.center.x,
                bounds.min.y + 1.0f,
                bounds.center.z
            );

            windowLight.transform.rotation =
                Quaternion.LookRotation(
                    target - windowLight.transform.position
                );
        }

        // ----------------------------------------
        // 5. მაგიდის სანათის თბილი შუქი
        // იმუშავებს, თუ ობიექტის სახელი შეიცავს
        // სიტყვას "lamp".
        // ----------------------------------------

        Transform lamp = FindByName(
            Room.transform,
            "lamp"
        );

        if (lamp != null)
        {
            CreateLight(
                atmosphere.transform,
                "Desk Lamp Light",
                LightType.Point,
                lamp.position + Vector3.up * 0.2f,
                new Color(1.0f, 0.72f, 0.40f),
                3.0f,
                4.0f,
                true
            );
        }

        // ----------------------------------------
        // 6. არსებული Global Volume-ის გამოყენება
        // ახალ Volume-ს არ ვქმნით, რათა ეფექტები
        // ზედმეტად არ გაძლიერდეს.
        // ----------------------------------------

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            UniversalAdditionalCameraData cameraData =
                mainCamera.GetUniversalAdditionalCameraData();

            Undo.RecordObject(
                cameraData,
                "Enable Post Processing"
            );

            cameraData.renderPostProcessing = true;
        }

        // ცვლილებების შენახვისთვის სცენის მონიშვნა
        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        Selection.activeGameObject = atmosphere;

        EditorUtility.DisplayDialog(
            "Detective",
            "განათება განახლდა!\n\n" +
            "ოთახის არსებული მოდელები და ტექსტურები " +
            "შენარჩუნებულია.\n\n" +
            "შეამოწმე შედეგი Scene View-ში.",
            "OK"
        );
    }

    // --------------------------------------------
    // ობიექტის ძებნა სახელის მიხედვით
    // --------------------------------------------

    private static Transform FindByName(
        Transform root,
        string searchTerm
    )
    {
        foreach (
            Transform child in
            root.GetComponentsInChildren<Transform>(true)
        )
        {
            if (
                child.name.IndexOf(
                    searchTerm,
                    System.StringComparison.OrdinalIgnoreCase
                ) >= 0
            )
            {
                return child;
            }
        }

        return null;
    }

    // --------------------------------------------
    // განათების შექმნა
    // --------------------------------------------

    private static Light CreateLight(
        Transform parent,
        string lightName,
        LightType type,
        Vector3 position,
        Color color,
        float intensity,
        float range,
        bool shadows
    )
    {
        GameObject lightObject = new GameObject(lightName);

        Undo.RegisterCreatedObjectUndo(
            lightObject,
            "Create " + lightName
        );

        lightObject.transform.SetParent(parent);
        lightObject.transform.position = position;

        Light lightComponent = lightObject.AddComponent<Light>();

        lightComponent.type = type;
        lightComponent.color = color;
        lightComponent.intensity = intensity;
        lightComponent.range = range;

        lightComponent.shadows = shadows
            ? LightShadows.Soft
            : LightShadows.None;

        return lightComponent;
    }
}

#endif
