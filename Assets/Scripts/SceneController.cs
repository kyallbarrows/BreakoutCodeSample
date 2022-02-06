using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    private Light _lavaLight;
    private Light _roomLight;

    private GameObject _lavaBlob1;
    private GameObject _lavaBlob2;
    private GameObject _lavaBlob3;

    private Vector3 _lavaBlob1Home;
    private Vector3 _lavaBlob2Home;
    private Vector3 _lavaBlob3Home;

    private float _lavaLightMaxIntensity;

    // Start is called before the first frame update
    void Start()
    {
        _roomLight = GameObject.Find("RoomLight").GetComponent<Light>();
        _lavaLight = GameObject.Find("LavaLight").GetComponent<Light>();
        _lavaLightMaxIntensity = _lavaLight.intensity;

        _lavaBlob1 = GameObject.Find("LavaBlob1");
        _lavaBlob2 = GameObject.Find("LavaBlob2");
        _lavaBlob3 = GameObject.Find("LavaBlob3");

        _lavaBlob1Home = _lavaBlob1.transform.localPosition;
        _lavaBlob2Home = _lavaBlob2.transform.localPosition;
        _lavaBlob3Home = _lavaBlob3.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        var t = Time.timeSinceLevelLoad;
        // randomly wave lava light's intensity between half and full intensity
        _lavaLight.intensity = (RandomWaves(t, .3f) * .8f + .2f) * _lavaLightMaxIntensity;
        _roomLight.enabled = Time.timeSinceLevelLoad % 40 > 35;

        var lavaBlob1Stretch = 1f + RandomWaves(t, .1f);
        var lavaBlob2Stretch = .5f + RandomWaves(t, .15f);
        var lavaBlob3Stretch = .8f + RandomWaves(t, .08f);
        _lavaBlob1.transform.localScale = new Vector3(1, 1, lavaBlob1Stretch);
        _lavaBlob2.transform.localScale = new Vector3(1, 1, lavaBlob2Stretch);
        _lavaBlob3.transform.localScale = new Vector3(1, 1, lavaBlob3Stretch);

        var lavaBlob1Y = RandomWaves(t, .1f) * .65f;
        var lavaBlob2Y = RandomWaves(t, .2f) * 2f;
        var lavaBlob3Y = RandomWaves(t, .15f) * 2f;
        _lavaBlob1.transform.localPosition = _lavaBlob1Home + new Vector3(0, lavaBlob1Y, 0);
        _lavaBlob2.transform.localPosition = _lavaBlob2Home + new Vector3(0, lavaBlob2Y, 0);
        _lavaBlob3.transform.localPosition = _lavaBlob3Home + new Vector3(0, lavaBlob3Y, 0);
    }

    /// <summary>
	/// Generates a value between 0 and 1, in a kinda-random wavy pattern
	/// </summary>
	/// <param name="t">Time in seconds</param>
	/// <param name="timeScale">Set to greater than 1 to stretch the wave out, so they are not all the same.</param>
	/// <returns></returns>
    private float RandomWaves(float t, float timeScale) {
        // a sine wave, plus a slightly shorter, but much wider sine wave, raised above 0, then
        // divided down to 0-1 range
        float value = (2f + 1.35f * Mathf.Sin(t * timeScale) + .65f * Mathf.Sin(t * timeScale * 4f)) / 4f;
        return value;
    }
}
