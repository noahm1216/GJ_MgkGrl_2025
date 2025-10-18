using UnityEngine;
using UnityEngine.UI;

public class SettingsGraphics : MonoBehaviour
{

    // change graphics 
    [Header("Quality\n__________")]
    public Dropdown qualityDropdown;
    [Range(0, 2)] public int qualityLevel = 0;
    public string[] qualityNames = { "Performance", "Balanced", "Fidelity" };


    // turn on/ off vsync
    [Header("VSynch\n__________")]
    public Toggle vsyncToggle;
    public bool vsyncEnabled = false;


    // set target framerate
    [Header("Framerate\n__________")]
    public Slider fpsSlider;
    public bool fpsCapped = false;
    [Range(24, 480)] public int fpsTarget = 120;

    // set target framerate
    [Header("Resolution\n__________")]
    public Dropdown resolutionDropdown;
    



    void SetGraphics()
    {
        // Apply Quality Level
        QualitySettings.SetQualityLevel(qualityLevel);

        // Apply VSync and Target FPS
        if (vsyncEnabled)
        {
            QualitySettings.vSyncCount = 1; // Enable VSync
            Application.targetFrameRate = -1; // VSync takes precedence, so targetFrameRate is ignored
        }
        else
        {
            QualitySettings.vSyncCount = 0; // Disable VSync
            if (fpsCapped)
                Application.targetFrameRate = fpsTarget; // Set custom target FPS
            else
                Application.targetFrameRate = 240; // hard cap to 240 if we are not setting it
        }
    }

    public void ChangeVSync() // TODO: write functions to set the variables based on the UI references. .. then call those references when we make changes
    {

    }
}
