using System.Collections;
using TMPro;
using UnityEngine;

public class GameLoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text tipText;

    [TextArea]
    [SerializeField] private string[] tips =
    {
        "Tip: blefuj ostrożnie.",
        "Tip: brak karty w ręce nie oznacza, że układu nie ma na stole.",
        "Tip: sprawdzanie w złym momencie daje karną kartę.",
        "Tip: obstawiać można tylko wyższym układem.",
        "Tip: im więcej kart dostaniesz, tym trudniej ukryć blef."
    };

    private Coroutine loadingDotsCoroutine;
    private string baseLoadingMessage = "Ładowanie graczy";

    private void Start()
    {
        ShowLoading("Ładowanie graczy");
        SetRandomTip();
    }

    private void LateUpdate()
    {
        if (loadingPanel != null && loadingPanel.activeSelf)
        {
            loadingPanel.transform.SetAsLastSibling();
        }
    }

    public void ShowLoading(string message = "Ładowanie graczy")
    {
        baseLoadingMessage = message.TrimEnd('.');

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            loadingPanel.transform.SetAsLastSibling();
        }

        if (loadingDotsCoroutine != null)
            StopCoroutine(loadingDotsCoroutine);

        loadingDotsCoroutine = StartCoroutine(AnimateLoadingDots());
    }

    public void HideLoading()
    {
        if (loadingDotsCoroutine != null)
        {
            StopCoroutine(loadingDotsCoroutine);
            loadingDotsCoroutine = null;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void SetRandomTip()
    {
        if (tipText == null || tips == null || tips.Length == 0)
            return;

        int randomIndex = Random.Range(0, tips.Length);
        tipText.text = tips[randomIndex];
    }

    private IEnumerator AnimateLoadingDots()
    {
        int dots = 1;

        while (true)
        {
            if (loadingText != null)
                loadingText.text = baseLoadingMessage + new string('.', dots);

            dots++;
            if (dots > 5)
                dots = 1;

            yield return new WaitForSeconds(1f);
        }
    }
}