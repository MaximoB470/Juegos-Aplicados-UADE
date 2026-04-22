using System.Collections.Generic;


public class EPPResult
{
    public bool headCorrect;
    public bool bodyCorrect;
    public bool handsCorrect;
    public bool feetCorrect;
    public bool allCorrect => headCorrect && bodyCorrect && handsCorrect && feetCorrect;

    public List<string> incorrectLabels = new List<string>();

    public List<string> correctLabels = new List<string>();

    public List<string> incorrectCategoryNames = new List<string>();

    public string scenarioFeedback;
}
