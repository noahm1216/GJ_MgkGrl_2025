using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SteamPlayerInfo : MonoBehaviour
{
    public RawImage profilePictureRawImage;
    public TextMeshProUGUI profileNameText;
    private bool checkedForData;

    private void LateUpdate()
    {
        if (!checkedForData)
        { SetProfilePicture(); checkedForData = true; }
    }

    public void SetProfilePicture()
    {
        if (Manager_Steam.Instance) {
            if (profilePictureRawImage) profilePictureRawImage.texture = Manager_Steam.Instance.GetAvatarPlayerImage();
            if (profileNameText) profileNameText.text = Manager_Steam.Instance.steamName;
        }
    }


}

