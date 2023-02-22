using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ProcedureGenerator : MonoBehaviour
{
    public Button generateProcedureButton;

    private string filePath;
    private string delimiter = ",";
    // Start is called before the first frame update
    void Start()
    {
        generateProcedureButton.onClick.AddListener(GenerateProcedure);

        filePath = Application.dataPath + "/example.csv";
    }
    
    void GenerateProcedure()
    {
        StreamWriter sw = new StreamWriter(filePath);

        foreach (SessionState.WellPlate step in SessionState.Steps)
        {
            sw.WriteLine("plate:horizontal", char.ConvertFromUtf32(160), char.ConvertFromUtf32(160), char.ConvertFromUtf32(160));
            foreach (var well in step.wells)
            {
                foreach (var liquid in well.Value.liquids)
                {
                    Debug.Log(well.Key + " " + liquid.color + " " + liquid.colorName + liquid.name);
                    sw.WriteLine(well.Key + delimiter + Color32ToHex(liquid.color).ToString() + delimiter + liquid.colorName + delimiter + liquid.name.Remove(liquid.name.Length - 1, 1) + delimiter +  "10" + delimiter + "μL");
                }
            }
        }

        sw.Close();

        Debug.Log("CSV file written to: " + filePath);
    }

    public static string Color32ToHex(Color32 color)
    {
        // Convert the color values to hexadecimal strings
        string hexR = color.r.ToString("X2");
        string hexG = color.g.ToString("X2");
        string hexB = color.b.ToString("X2");

        // Combine the hexadecimal strings into a single color code
        string hexColor = "#" + hexR + hexG + hexB;

        return hexColor;
    }
}
