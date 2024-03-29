﻿using UnityEngine;
using UnityEngine.UI;

public class StartConfigurationScreen2 : MonoBehaviour
{
    public Toggle useSaveDataToggle;

    public Text populationCountTxt;
    public Slider populationCountSlider;
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
    public GameObject simulationScreen;

    string populationText;
    string elitesText;
    string mutationChanceText;
    string mutationRateText;
    string hiddenLayersCountText;
    string biasText;
    string sigmoidSlopeText;
    string neuronsPerHLCountText;
    string inputsText;
    string outputsText;

    void Start()
    {
        useSaveDataToggle.onValueChanged.AddListener(active =>
        {
            PopulationManager.Instance.useSavedGenomes = active;
        });

        populationCountSlider.onValueChanged.AddListener(OnPopulationCountChange);
        eliteCountSlider.onValueChanged.AddListener(OnEliteCountChange);
        mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceChange);
        mutationRateSlider.onValueChanged.AddListener(OnMutationRateChange);
        hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountChange);
        neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLChange);
        biasSlider.onValueChanged.AddListener(OnBiasChange);
        sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeChange);
        inputsSlider.onValueChanged.AddListener(OnInputsCountChange);
        outputsSlider.onValueChanged.AddListener(OnOutputsCountChange);

        populationText = populationCountTxt.text;
        elitesText = eliteCountTxt.text;
        mutationChanceText = mutationChanceTxt.text;
        mutationRateText = mutationRateTxt.text;
        hiddenLayersCountText = hiddenLayersCountTxt.text;
        neuronsPerHLCountText = neuronsPerHLCountTxt.text;
        biasText = biasTxt.text;
        sigmoidSlopeText = sigmoidSlopeTxt.text;
        inputsText = inputsCountTxt.text;
        outputsText = outputsCountTxt.text;

        populationCountSlider.value = PopulationManager.Instance.agent2Config.PopulationCount;
        eliteCountSlider.value = PopulationManager.Instance.agent2Config.EliteCount;
        mutationChanceSlider.value = PopulationManager.Instance.agent2Config.MutationChance * 100.0f;
        mutationRateSlider.value = PopulationManager.Instance.agent2Config.MutationRate * 100.0f;
        hiddenLayersCountSlider.value = PopulationManager.Instance.agent2Config.HiddenLayers;
        neuronsPerHLSlider.value = PopulationManager.Instance.agent2Config.NeuronsCountPerHL;
        biasSlider.value = PopulationManager.Instance.agent2Config.Bias;
        sigmoidSlopeSlider.value = PopulationManager.Instance.agent2Config.P;
        inputsSlider.value = PopulationManager.Instance.agent2Config.InputsCount;
        outputsSlider.value = PopulationManager.Instance.agent2Config.OutputsCount;
    }

    void OnInputsCountChange(float value)
    {
        PopulationManager.Instance.agent2Config.InputsCount = (int)value;

        inputsCountTxt.text = string.Format(inputsText, PopulationManager.Instance.agent2Config.InputsCount);
    }
    
    void OnOutputsCountChange(float value)
    {
        PopulationManager.Instance.agent2Config.OutputsCount = (int)value;

        outputsCountTxt.text = string.Format(outputsText, PopulationManager.Instance.agent2Config.OutputsCount);
    }
    
    void OnPopulationCountChange(float value)
    {
        PopulationManager.Instance.agent2Config.PopulationCount = (int)value;

        populationCountTxt.text = string.Format(populationText, PopulationManager.Instance.agent2Config.PopulationCount);
    }

    void OnEliteCountChange(float value)
    {
        PopulationManager.Instance.agent2Config.EliteCount = (int)value;

        eliteCountTxt.text = string.Format(elitesText, PopulationManager.Instance.agent2Config.EliteCount);
    }

    void OnMutationChanceChange(float value)
    {
        PopulationManager.Instance.agent2Config.MutationChance = value / 100.0f;

        mutationChanceTxt.text = string.Format(mutationChanceText, (int)(PopulationManager.Instance.agent2Config.MutationChance * 100));
    }

    void OnMutationRateChange(float value)
    {
        PopulationManager.Instance.agent2Config.MutationRate = value / 100.0f;

        mutationRateTxt.text = string.Format(mutationRateText, (int)(PopulationManager.Instance.agent2Config.MutationRate * 100));
    }

    void OnHiddenLayersCountChange(float value)
    {
        PopulationManager.Instance.agent2Config.HiddenLayers = (int)value;

        hiddenLayersCountTxt.text = string.Format(hiddenLayersCountText, PopulationManager.Instance.agent2Config.HiddenLayers);
    }

    void OnNeuronsPerHLChange(float value)
    {
        PopulationManager.Instance.agent2Config.NeuronsCountPerHL = (int)value;

        neuronsPerHLCountTxt.text = string.Format(neuronsPerHLCountText, PopulationManager.Instance.agent2Config.NeuronsCountPerHL);
    }

    void OnBiasChange(float value)
    {
        PopulationManager.Instance.agent2Config.Bias = -value;

        biasTxt.text = string.Format(biasText, PopulationManager.Instance.agent2Config.Bias.ToString("0.00"));
    }

    void OnSigmoidSlopeChange(float value)
    {
        PopulationManager.Instance.agent2Config.P = value;

        sigmoidSlopeTxt.text = string.Format(sigmoidSlopeText, PopulationManager.Instance.agent2Config.P.ToString("0.00"));
    }

    void OnStartButtonClick()
    {
        PopulationManager.Instance.StartSimulation();
        this.gameObject.SetActive(false);
        simulationScreen.SetActive(true);
    }
}