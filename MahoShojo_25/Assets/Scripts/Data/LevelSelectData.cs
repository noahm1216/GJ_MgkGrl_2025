using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class LevelSelectData : MonoBehaviour
{
    public Button btnImage;
    public TextMeshProUGUI textBtnName;
    public Image imgMonster;
    public Sprite spriteBtnLock, spriteBtnUnlock;

   public void UpdateLevelButton(string _btnName, bool _btnUnlocked, Sprite _monsterImg, bool _beatLevel)
    {
        if (textBtnName) textBtnName.text = _btnName; // change the name of the button text

        if (btnImage) // change the image of button
        {
            if (_btnUnlocked && spriteBtnUnlock) btnImage.image.sprite = spriteBtnUnlock;
            if(!_btnUnlocked && spriteBtnLock) btnImage.image.sprite = spriteBtnLock;
            btnImage.enabled = _btnUnlocked;
        }       
        
        if (imgMonster) // change the image of the monster
        {
            if (_monsterImg) imgMonster.sprite = _monsterImg;
            if (_beatLevel) imgMonster.color = Color.white;
            else imgMonster.color = Color.black;
        }
    }

}
