#r "System.Xml.Linq"

using System.Xml;
using System.Xml.Linq;

/*
Point to the ExpressionTemplate.xml file to use.
*/
var ExpressionTemplatesXml = ReadFile(Model.GetAnnotation("MeasureMagic_FileLocation"));

/*
Overwrite existing Measures?
*/
var overwriteExistingMeasures = false;

/*
Copy DAX expression to description, if created measure has no description?
*/
var setEmptyDescriptionToExpression = false;

/*
Set to false if the domain daxformatter.com is not reachable.
Disabling Formatting will also speed up creation of Measures. 
*/
var tryFormattingDAXExpressions = true;

/*
Enable logging (will create a lot of Message Boxes if you are not on the command line). 
*/
var debugOutput = false;

string[] enabledArray = { "1", "true", "True" };
string[] disabledArray = { "0", "false", "False" };

XDocument MeasureTemplateGroups = XDocument.Parse(ExpressionTemplatesXml);

var measureTemplates =
    MeasureTemplateGroups
    .Descendants("MeasureTemplateGroup")
    .Where(x => enabledArray.Contains((string)x.Attribute("Enabled")))
    .Descendants("MeasureCollection")
    .Where(x => enabledArray.Contains((string)x.Attribute("Enabled")))
    .Descendants("Measure")
    .Where(x => enabledArray.Contains((string)x.Attribute("Enabled")))
    .Select(template => new
    {
        NameTemplate = (string)template.Attribute("NameTemplate"),
        TranslatedName = (string)template.Attribute("TranslatedName"),
        Description = (string)template.Attribute("Description"),
        DisplayFolder = (string)template.Attribute("DisplayFolder"),
        FormatString = (string)template.Attribute("FormatString"),
        IsHidden = (string)template.Attribute("IsHidden"),
        ExpressionTemplate = (string)template.Value
    });

foreach (var template in measureTemplates)
{
    var vNameTemplate = template.NameTemplate;
    var vExpressionTemplate = template.ExpressionTemplate;

    foreach (var baseMeasure in Selected.Measures)
    {
        var vDatenstandOrTable =
            String.IsNullOrEmpty(baseMeasure.Table.GetExtendedProperty("QuelleDatenstand")) ?
            baseMeasure.Table.Name :
            baseMeasure.Table.GetExtendedProperty("QuelleDatenstand");

        var vMName = String.Format(
            vNameTemplate,
            baseMeasure.Name,
            baseMeasure.Table.Name,
            vDatenstandOrTable
        );

        if (baseMeasure.Table.Measures.Contains(vMName))
        {
            if (overwriteExistingMeasures)
            {
                baseMeasure.Table.Measures[vMName].Delete();
            }
            else
            {
                if (debugOutput)
                {
                    Info(String.Format(@"Measure [{0}] already exists, ommiting.", vMName));
                }
                continue;
            }
        }

        var vMExpression = String.Format(
            vExpressionTemplate,
            baseMeasure.DaxObjectName,
            baseMeasure.Name,
            baseMeasure.Description,
            vDatenstandOrTable
        );

        var measure = baseMeasure.Table.AddMeasure(
            vMName,
            vMExpression
        );

        measure.Description = String.Format(
            template.Description ?? "",
            baseMeasure.Name
        );

        if (
            (setEmptyDescriptionToExpression && String.IsNullOrEmpty(measure.Description)) ||
            measure.GetAnnotation("ExpressionAsDescription") == "1"
        )
        {
            measure.Description = measure.Expression;
            measure.SetAnnotation("ExpressionAsDescription", "1");
        }

        measure.DisplayFolder =
            template.DisplayFolder is object ?
            String.Format(
                template.DisplayFolder,
                baseMeasure.Name,
                baseMeasure.DisplayFolder
            ) : baseMeasure.DisplayFolder;

        measure.IsHidden = String.IsNullOrEmpty(template.IsHidden) ? false : bool.Parse(template.IsHidden);
        measure.FormatString = template.FormatString ?? baseMeasure.FormatString;
        measure.InPerspective.CopyFrom(baseMeasure.InPerspective);

        foreach (var c in Model.Cultures)
        {
            var tmpTranslatedName =
                template.TranslatedName is object ?
                    String.Format(
                        template.TranslatedName,
                        baseMeasure.Name,
                        baseMeasure.Table.Name,
                        vDatenstandOrTable,
                        baseMeasure.TranslatedNames[c] is string &&
                            baseMeasure.TranslatedNames[c].Length > 0 ?
                                baseMeasure.TranslatedNames[c] : baseMeasure.Name
                    ) : null;

            if (tmpTranslatedName != vMName)
            {
                measure.TranslatedNames[c] = tmpTranslatedName;
            }

        }
        
        if (tryFormattingDAXExpressions)
        {
            measure.FormatDax();
        }
        
        measure.SetAnnotation("AUTOGEN", "1");

        if (debugOutput)
        {
            Info(String.Format(@"Measure {0} created.", measure.DaxObjectName));
        }
    }
}