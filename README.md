# MeasureMagic

Create measures from templates with TabularEditor

## Contents

- [MeasureMagic](#measuremagic)
  - [Contents](#contents)
  - [Getting started](#getting-started)
  - [MeasureMagic.xml Structure](#measuremagicxml-structure)
  - [Implemented template variables](#implemented-template-variables)
    - [DAX expression](#dax-expression)
      - [Examples for DAX expression](#examples-for-dax-expression)
    - [NameTemplate and Description](#nametemplate-and-description)
      - [Examples for NameTemplate and Description](#examples-for-nametemplate-and-description)
    - [DisplayFolder](#displayfolder)
      - [Examples for DisplayFolder](#examples-for-displayfolder)
  - [Features](#features)
  - [TODO](#todo)

## Getting started

- Open your model in Tabular Editor.
- Add an annotation with the name `MeasureTemplates_FileLocation` to your Model.
- Put the absolute path of your `MeasureMagic.xml` in the value of the annotation.
- Add new MeasureTemplateGroup to `MeasureMagic.xml`, disable all unwanted MeasureTemplateGroups, MeasureTemplate-Collections and MeasureTemplates
- Load MeasureMagic.cs advanced script
- Select one or more measures
- Hit `F5`

## MeasureMagic.xml Structure

The XML document has the following structure:

    <MeasureTemplateGroups>
        <MeasureTemplateGroup Enabled="true" Name="SeRA">
            <MeasureTemplates Enabled="true">
                <MeasureTemplate Enabled="true" NameTemplate="Datenstand Datum {2}" DisplayFolder="" Description="" IsHidden="true" FormatString="dd.MM.yyyy">
                    TODAY()
                </MeasureTemplate>
                ...
            </MeasureTemplates>
            ...
        </MeasureTemplateGroup>
        ...
    </MeasureTemplateGroups>

The file needs to have exactly one `MeasureTemplateGroups` node as the XML-root.

The root can contain one or many `MeasureTemplateGroup` nodes. Each of the groups can be  disabled with the `Enabled` property set to `false`. The `Name` property of the group is only for your own reference, it will not be used for any processing as of now.

Next level is the `MeasureTemplates` node which can be used to group the templates further, e.g. one group for Time Intelligence, another one for descriptive measures, etc. The `MeasureTemplates` can be disabled with the `Enabled` property set to `false`.

`MeasureTemplate` nodes are on the lowest level and allow for several attributes:

- `Enabled`: set to `false` to disable this template in the next run
- `NameTemplate`: name of the measure created
- `DisplayFolder`: display folder of the measure - default: inherits from the base measure
- `Description`: description of the measure - default: empty
- `IsHidden`: is the new measure hidden? - default: false
- `FormatString`: provide a FormatString for the measure - default: inherits from the base measure

The DAX expression of the measure goes into the value of the node.
If the expression contains xml-special characters `<` or `>` the expression needs to be wrapped in `<![CDATA[` and `]]>`.

For example:

    <MeasureTemplate Enabled="true" NameTemplate="{0} Total" DisplayFolder="" Description="" IsHidden="true" FormatString="dd.MM.yyyy">
        <![CDATA[
            CALCULATE(
                {0},
                ALL( DimDate ),
                DimDate[Date] < MAX( Orders[OrderDate] ) 
            )
        ]]>
    </MeasureTemplate>

## Implemented template variables

The attributes NameTemplate, DisplayFolder and Description and of course the DAX expression allow for several variables that will be replaced.

### DAX expression

- `{0}` = Base Measure Dax Expression, e.g. "[Base Measure]"
- `{1}` = Base Measure Name, e.g. "Base Measure"
- `{2}` = Value of the Description property of the base measure
- `{3}` = Table Name Datenstand or Table Name of Base Measure as fallback

#### Examples for DAX expression

tbc.

### NameTemplate and Description

- `{0}` = Base Measure Name, e.g. "Base Measure"
- `{1}` = Base Measure Table Name, e.g. "Fact MeConnect"
- `{2}` = Table Name Datenstand or Table Name of Base Measure as fallback

#### Examples for NameTemplate and Description

tbc.

### DisplayFolder

- `{0}` = Base Measure Name, e.g. "Net Revenue"
- `{1}` = Base Measure Display Folder, e.g. "Net Revenue\Time Intelligence"

#### Examples for DisplayFolder

- Put the measure in the "Time Intelligence" folder in the "root" of the table:
  
        DisplayFolder="Time Intelligence"

- Put the measure at the same folder as the base measure:
  
        DisplayFolder="{1}"
  
    > This is the default, with no DisplayFolder attribute set the folder of the base measure will be inherited.

- Put the measure at the subfolder "Time Intelligence" below the folder of the base measure

        DisplayFolder="{1}\Time Intelligence"

- Put the measure at the root of the table

        DisplayFolder=""

    > Set the DisplayFolder to "" if the base measure is within a folder but the new measure should be located at the "root" of the table.

## Features

- replacement of variables within templates
- easy to read/write DAX expressions (no escaping etc)
- annotate Model
- self-documenting templates

## TODO

- tbc.
