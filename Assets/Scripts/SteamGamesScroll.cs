using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
#if TMP_PRESENT
using TMPro;
#endif

public class SteamGamesScroll : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentRoot; // ScrollView/Viewport/Content
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Texture2D fallbackTexture;

    [Serializable]
    public class OwnedGamesResponse
    {
        public OwnedGamesData response;
    }

    [Serializable]
    public class OwnedGamesData
    {
        public int game_count;
        public List<OwnedGame> games;
    }

    [Serializable]
    public class OwnedGame
    {
        public int appid;
        public string name;
        public int playtime_forever; // minutes
        public string img_icon_url;
        public string img_logo_url;
    }

    private void Start()
    {
        StartCoroutine(LoadAndBuild());
    }

    private IEnumerator LoadAndBuild()
    {
        // In WebGL this is a URL; in Editor it still works.
        string url = System.IO.Path.Combine(Application.streamingAssetsPath, "steamGamesFile.json");

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load StreamingAssets JSON: {req.error} @ {url}");
                yield break;
            }

            string json = req.downloadHandler.text;
            var data = JsonUtility.FromJson<OwnedGamesResponse>(json);

            if (data?.response?.games == null || data.response.games.Count == 0)
            {
                Debug.LogWarning("No games found in JSON.");
                yield break;
            }

            // sort by hours desc
            data.response.games.Sort((a, b) => b.playtime_forever.CompareTo(a.playtime_forever));

            foreach (var g in data.response.games)
                CreateItem(g);
        }
    }

    private void CreateItem(OwnedGame g)
    {
        var go = Instantiate(itemPrefab, contentRoot);

#if TMP_PRESENT
    TMP_Text hoursText = go.transform.Find("hours")?.GetComponent<TMP_Text>();
    if (hoursText != null)
        hoursText.text = $"{g.playtime_forever / 60f:0.0} h";
#else
        TextMeshProUGUI hoursText = go.transform.Find("hours")?.GetComponent<TextMeshProUGUI>();
        if (hoursText != null)
            hoursText.text = $"{g.playtime_forever / 60f:0.0} hours";
#endif

        RawImage img = go.GetComponentInChildren<RawImage>();
        if (img != null)
            StartCoroutine(LoadBestImage(g.appid, img));

    }

    private IEnumerator LoadBestImage(int appid, RawImage target)
    {
        var urls = new[]
        {
        // 1) Portrait capsule (like Steam client library)
        $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/library_600x900.jpg",
        // 2) Regular header (wide)
        $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg",
        // 3) Larger horizontal capsule (fallbacks)
        $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/capsule_616x353.jpg",
        $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/capsule_467x181.jpg",
        $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/capsule_231x87.jpg",
        $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/capsule_184x69.jpg"
    };

        foreach (var url in urls)
        {
            using (var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
            {
                req.timeout = 10;
                yield return req.SendWebRequest();

                if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
                    if (tex != null && target != null)
                    {
                        target.texture = tex;

                        // Preserve aspect & avoid distortion
                        var arf = target.GetComponent<AspectRatioFitter>() ?? target.gameObject.AddComponent<AspectRatioFitter>();
                        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                        arf.aspectRatio = (float)tex.width / tex.height;
                    }
                    yield break;
                }
            }
        }

        // absolute fallback
        if (target != null && fallbackTexture != null)
        {
            target.texture = fallbackTexture;
            var arf = target.GetComponent<AspectRatioFitter>() ?? target.gameObject.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = (float)fallbackTexture.width / fallbackTexture.height;
        }
    }





    private System.Collections.IEnumerator LoadImage(string url, RawImage target, Action onFail = null)
    {
        using (var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
                target.texture = tex;
            }
            else
            {
                onFail?.Invoke();
            }
        }
    }
}
