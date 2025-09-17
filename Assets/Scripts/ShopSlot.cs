using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Image animalIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Button buyButton;

    private UnitData currentUnitData; // 你的动物 ScriptableObject

    void Start()
    {
        // 绑定按钮点击事件
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    // ShopManager 会调用这个方法来刷新商店
    public void DisplayUnit(UnitData data)
    {
        currentUnitData = data;
        animalIcon.sprite = data.shopIcon; // 假设 UnitData 里有商店图标
        nameText.text = data.unitName;
        costText.text = data.cost.ToString(); // 假设 UnitData 里有 cost
        buyButton.interactable = true;
    }

    void OnBuyClicked()
    {
        Debug.Log("尝试购买 " + currentUnitData.unitName);
        // 1. 检查金币是否足够
        if (GameManager.Instance.PlayerGold >= currentUnitData.cost)
        {
            // 2. 尝试添加到备战席
            bool success = BenchManager.Instance.AddUnitToBench(currentUnitData);

            if (success)
            {
                // 3. 扣钱
                GameManager.Instance.SpendGold(currentUnitData.cost);

                // 4. (可选) 让这个槽位变灰，表示已购买
                buyButton.interactable = false;
            }
            else
            {
                Debug.Log("备战席满了！");
                // TODO: 提示玩家备战席已满
            }
        }
        else
        {
            Debug.Log("金币不足！");
            // TODO: 提示玩家金币不足
        }
    }
}