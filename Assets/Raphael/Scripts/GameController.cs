using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    [SerializeField] private TMP_Text clientNameText;
    [SerializeField] private TMP_Text contractNameText;
    [SerializeField] private TMP_Text contractDescriptionText;
    [SerializeField] private Button doomButton;
    [SerializeField] private Button dealButton;

    [SerializeField] private StatsBarController barsController;

    private float currentDamnation = 1f;
    private float currentSatisfaction = 0f;
    private float currentHeat = 0f;


    private List<ContractData> contractDataList;
    private int currentContractIndex = 0;

    [SerializeField] private TextAsset contractsJson;

    void Start()
    {
        ContractDataList dataList = JsonUtility.FromJson<ContractDataList>(contractsJson.text);
        contractDataList = dataList.contracts;

        doomButton.onClick.AddListener(() => OnContractChosen(false));
        dealButton.onClick.AddListener(() => OnContractChosen(true));

        barsController.UpdateBars(currentDamnation, currentHeat, currentSatisfaction);
        DisplayCurrentContract();
    }


    private void DisplayCurrentContract()
    {
        if (currentContractIndex >= contractDataList.Count)
        {
            Debug.Log("All contracts completed!");
            // TODO: Handle game end
            return;
        }

        var currentContract = contractDataList[currentContractIndex];
        clientNameText.text = currentContract.clientName;
        contractNameText.text = currentContract.contractName;
        contractDescriptionText.text = currentContract.description;
    }

    private void OnContractChosen(bool isDeal)
    {
        var currentContract = contractDataList[currentContractIndex];

        if (isDeal)
        {
            currentDamnation = Mathf.Clamp01(currentDamnation + currentContract.damnation);
            currentSatisfaction = Mathf.Clamp01(currentSatisfaction + currentContract.satisfaction);
            currentHeat = Mathf.Clamp01(currentHeat + currentContract.heat);
        }
        else
        {
            float doomDamnation = currentContract.damnation >= 0 ? -currentContract.damnation : currentContract.damnation * 2f;
            currentDamnation = Mathf.Clamp01(currentDamnation + doomDamnation);
            currentSatisfaction = Mathf.Clamp01(currentSatisfaction + currentContract.satisfaction);
            currentHeat = Mathf.Clamp01(currentHeat + currentContract.heat);
        }

        barsController.UpdateBars(currentDamnation, currentHeat, currentSatisfaction);

        currentContractIndex++;
        DisplayCurrentContract();
    }





    void Update()
    {
        
    }
}
