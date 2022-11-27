using UnityEngine;
using UnityEngine.UI;

public class StartConfigurationScreen1 : MonoBehaviour
{
    public Text gridSizeTxt;
    public Slider gridSizeSlider;
    public Text populationCountTxt;
    public Slider populationCountSlider;
    public Slider populationCountSlider2;
    public Text totalTurnsTxt;
    public Slider totalTurnsSlider;
    public Text eliteCountTxt;
    public Slider eliteCountSlider;
    public Text mutationChanceTxt;
    public Slider mutationChanceSlider;
    public Text mutationRateTxt;
    public Slider mutationRateSlider;
    public Text hiddenLayersCountTxt;
    public Slider hiddenLayersCountSlider;
    public Text neuronsPerHLCountTxt;
    public Slider neuronsPerHLSlider;
    public Text inputsCountTxt;
    public Slider inputsSlider; 
    public Text outputsCountTxt;
    public Slider outputsSlider;
    public Text biasTxt;
    public Slider biasSlider;
    public Text sigmoidSlopeTxt;
    public Slider sigmoidSlopeSlider;
    public Button startButton;
    public GameObject simulationScreen;
    public GameObject configurationScreen2;

    string gridSizeText;
    string populationText;
    string totalTurnsText;
    string elitesText;
    string mutationChanceText;
    string mutationRateText;
    string hiddenLayersCountText;
    string biasText;
    string inputsText;
    string outputsText;
    string sigmoidSlopeText;
    string neuronsPerHLCountText;

    void Start()
    {
        gridSizeSlider.onValueChanged.AddListener(OnGridSizeCountChange);
        populationCountSlider.onValueChanged.AddListener(OnPopulationCountChange);
        totalTurnsSlider.onValueChanged.AddListener(OnTotalTurnsChange);
        eliteCountSlider.onValueChanged.AddListener(OnEliteCountChange);
        mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceChange);
        mutationRateSlider.onValueChanged.AddListener(OnMutationRateChange);
        hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountChange);
        neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLChange);
        biasSlider.onValueChanged.AddListener(OnBiasChange);
        sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeChange);
        inputsSlider.onValueChanged.AddListener(OnInputsCountChange);
        outputsSlider.onValueChanged.AddListener(OnOutputsCountChange);

        gridSizeText = gridSizeTxt.text;
        populationText = populationCountTxt.text;
        totalTurnsText = totalTurnsTxt.text;
        elitesText = eliteCountTxt.text;
        mutationChanceText = mutationChanceTxt.text;
        mutationRateText = mutationRateTxt.text;
        hiddenLayersCountText = hiddenLayersCountTxt.text;
        neuronsPerHLCountText = neuronsPerHLCountTxt.text;
        biasText = biasTxt.text;
        sigmoidSlopeText = sigmoidSlopeTxt.text;
        inputsText = inputsCountTxt.text;
        outputsText = outputsCountTxt.text;

        populationCountSlider.value = PopulationManager.Instance.agent1Config.PopulationCount;
        totalTurnsSlider.value = PopulationManager.Instance.TotalTurns;
        eliteCountSlider.value = PopulationManager.Instance.agent1Config.EliteCount;
        mutationChanceSlider.value = PopulationManager.Instance.agent1Config.MutationChance * 100.0f;
        mutationRateSlider.value = PopulationManager.Instance.agent1Config.MutationRate * 100.0f;
        hiddenLayersCountSlider.value = PopulationManager.Instance.agent1Config.HiddenLayers;
        neuronsPerHLSlider.value = PopulationManager.Instance.agent1Config.NeuronsCountPerHL;
        biasSlider.value = PopulationManager.Instance.agent1Config.Bias;
        sigmoidSlopeSlider.value = PopulationManager.Instance.agent1Config.P;
        gridSizeSlider.value = PopulationManager.Instance.GridHeight;
        inputsSlider.value = PopulationManager.Instance.agent1Config.InputsCount;
        outputsSlider.value = PopulationManager.Instance.agent1Config.OutputsCount;

        startButton.onClick.AddListener(OnStartButtonClick);
    }

    void OnInputsCountChange(float value)
    {
        PopulationManager.Instance.agent1Config.InputsCount = (int)value;

        inputsCountTxt.text = string.Format(inputsText, PopulationManager.Instance.agent1Config.InputsCount);
    }
    
    void OnOutputsCountChange(float value)
    {
        PopulationManager.Instance.agent1Config.OutputsCount = (int)value;

        outputsCountTxt.text = string.Format(outputsText, PopulationManager.Instance.agent1Config.OutputsCount);
    }
    
    void OnGridSizeCountChange(float value)
    {
        populationCountSlider.maxValue = value;
        populationCountSlider2.maxValue = value;
        
        PopulationManager.Instance.GridHeight = (int)value;
        PopulationManager.Instance.GridWidth = (int)value;

        gridSizeTxt.text = string.Format(gridSizeText, PopulationManager.Instance.GridHeight, PopulationManager.Instance.GridWidth);
    }
    
    void OnPopulationCountChange(float value)
    {
        PopulationManager.Instance.agent1Config.PopulationCount = (int)value;

        populationCountTxt.text = string.Format(populationText, PopulationManager.Instance.agent1Config.PopulationCount);
    }

    void OnTotalTurnsChange(float value)
    {
        PopulationManager.Instance.TotalTurns = (int)value;

        totalTurnsTxt.text = string.Format(totalTurnsText, PopulationManager.Instance.TotalTurns);
    }

    void OnEliteCountChange(float value)
    {
        PopulationManager.Instance.agent1Config.EliteCount = (int)value;

        eliteCountTxt.text = string.Format(elitesText, PopulationManager.Instance.agent1Config.EliteCount);
    }

    void OnMutationChanceChange(float value)
    {
        PopulationManager.Instance.agent1Config.MutationChance = value / 100.0f;

        mutationChanceTxt.text = string.Format(mutationChanceText, (int)(PopulationManager.Instance.agent1Config.MutationChance * 100));
    }

    void OnMutationRateChange(float value)
    {
        PopulationManager.Instance.agent1Config.MutationRate = value / 100.0f;

        mutationRateTxt.text = string.Format(mutationRateText, (int)(PopulationManager.Instance.agent1Config.MutationRate * 100));
    }

    void OnHiddenLayersCountChange(float value)
    {
        PopulationManager.Instance.agent1Config.HiddenLayers = (int)value;

        hiddenLayersCountTxt.text = string.Format(hiddenLayersCountText, PopulationManager.Instance.agent1Config.HiddenLayers);
    }

    void OnNeuronsPerHLChange(float value)
    {
        PopulationManager.Instance.agent1Config.NeuronsCountPerHL = (int)value;

        neuronsPerHLCountTxt.text = string.Format(neuronsPerHLCountText, PopulationManager.Instance.agent1Config.NeuronsCountPerHL);
    }

    void OnBiasChange(float value)
    {
        PopulationManager.Instance.agent1Config.Bias = -value;

        biasTxt.text = string.Format(biasText, PopulationManager.Instance.agent1Config.Bias.ToString("0.00"));
    }

    void OnSigmoidSlopeChange(float value)
    {
        PopulationManager.Instance.agent1Config.P = value;

        sigmoidSlopeTxt.text = string.Format(sigmoidSlopeText, PopulationManager.Instance.agent1Config.P.ToString("0.00"));
    }

    void OnStartButtonClick()
    {
        PopulationManager.Instance.StartSimulation();
        configurationScreen2.SetActive(false);
        this.gameObject.SetActive(false);
        simulationScreen.SetActive(true);
    }
}