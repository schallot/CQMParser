# CQMParser

Electronic Clinical Quality Measures are defined in HTML docs provided at https://ecqi.healthit.gov.  For example,
[CMS72v10](https://ecqi.healthit.gov/sites/default/files/ecqm/measures/CMS72v10.html).
These documents primarially define how patient populations are calculated for the metric, which involves a fairly well-structured pseudocode involving nested definitions.

This tool is designed to take an eCQM document, and recursively find and replace as many of the concepts as possible so that the population criteria are spelled out as explicitly as possible.  For example, for CMS72v10, the numerator is expressed as 

	"Encounter with Antithrombotic Therapy"

This tool [expands this line](https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/CMS72v10.txt) out as

	TJC.["Encounter, Performed: Non-Elective Inpatient Encounter" using "Non-Elective Inpatient Encounter (2.16.840.1.113883.3.117.1.7.1.424)"] NonElectiveEncounter
			where difference in days between start of Value and end of Value ( NonElectiveEncounter.relevantPeriod ) <= 120
				and NonElectiveEncounter.relevantPeriod ends during day of Measurement Period NonElectiveEncounter
			where exists ( NonElectiveEncounter.diagnoses Diagnosis
					where Diagnosis.rank = 1
						and ( Diagnosis.code in valueset "Hemorrhagic Stroke" (2.16.840.1.113883.3.117.1.7.1.212)
								or Diagnosis.code in valueset "Ischemic Stroke" (2.16.840.1.113883.3.117.1.7.1.247)
						)
			) AllStrokeEncounter
			with ["Patient Characteristic Birthdate: Birth date" using "Birth date (LOINC Code 21112-8)"] BirthDate
				such that years between ToDate(BirthDateTime)and ToDate(AsOf) ( BirthDate.birthDatetime, start of AllStrokeEncounter.relevantPeriod ) >= 18 EncounterWithAge
			where exists ( EncounterWithAge.diagnoses Diagnosis
					where Diagnosis.code in valueset "Ischemic Stroke" (2.16.840.1.113883.3.117.1.7.1.247)
						and Diagnosis.rank = 1
			) IschemicStrokeEncounter
			with ["Medication, Administered: Antithrombotic Therapy" using "Antithrombotic Therapy (2.16.840.1.113762.1.4.1110.62)"] Antithrombotic
				such that if pointInTime is not null then Interval[pointInTime, pointInTime]
			else if period is not null then period 
			else null as Interval<DateTime> ( Antithrombotic.relevantDatetime, Antithrombotic.relevantPeriod ) starts during Interval[DateTime(year from Value, month from Value, day from Value, 0, 0, 0, 0, timezoneoffset from Value) ( StartValue ), DateTime(year from Value, month from Value, day from Value, 0, 0, 0, 0, timezoneoffset from Value) ( StartValue + 2 days ) ) ( start of Encounter Visit
			let ObsVisit: Last(["Encounter, Performed: Observation Services" using "Observation Services (2.16.840.1.113762.1.4.1111.143)"] LastObs
					where LastObs.relevantPeriod ends 1 hour or less on or before start of Visit.relevantPeriod
					sort by 
					end of relevantPeriod
			),
			VisitStart: Coalesce(start of ObsVisit.relevantPeriod, start of Visit.relevantPeriod),
			EDVisit: Last(["Encounter, Performed: Emergency Department Visit" using "Emergency Department Visit (2.16.840.1.113883.3.117.1.7.1.292)"] LastED
					where LastED.relevantPeriod ends 1 hour or less on or before VisitStart
					sort by 
					end of relevantPeriod
			)
			return Interval[Coalesce(start of EDVisit.relevantPeriod, VisitStart), 
			end of Visit.relevantPeriod] ( IschemicStrokeEncounter ) )

## Usage
If run with no command-line arguments, the tool looks at the [CMS folder](https://github.com/schallot/CQMParser/tree/master/CMS), and processes all of the HTML files found there.  You can also pass in the path to a specific file, or the URL of a measure.  For example,

	CQMParse https://ecqi.healthit.gov/sites/default/files/ecqm/measures/CMS104v11.html
	
should generate an output similar to [CMS104v11.txt](https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/CMS104v11.txt).  The results will be written to the CompiledMeasures directory.  If a file is already present for the measure, it will be overwritten.
## Version Comparisons
It can sometimes be useful to compare versions of the compiled measures.  To aid with this, I've included WinMerge diffs of the two most recent versions of each measure:

* [CMS2 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS2Comparison_11-12.html)
* [CMS9 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS9Comparison_10-11.html)
* [CMS22 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS22Comparison_10-11.html)
* [CMS50 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS50Comparison_10-11.html)
* [CMS52 v7 vs v8](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS52Comparison_7-8.html)
* [CMS56 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS56Comparison_10-11.html)
* [CMS66 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS66Comparison_10-11.html)
* [CMS68 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS68Comparison_11-12.html)
* [CMS69 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS69Comparison_10-11.html)
* [CMS71 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS71Comparison_11-12.html)
* [CMS72 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS72Comparison_10-11.html)
* [CMS74 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS74Comparison_11-12.html)
* [CMS75 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS75Comparison_10-11.html)
* [CMS82 v6 vs v7](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS82Comparison_6-7.html)
* [CMS90 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS90Comparison_11-12.html)
* [CMS104 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS104Comparison_10-11.html)
* [CMS105 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS105Comparison_10-11.html)
* [CMS108 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS108Comparison_10-11.html)
* [CMS117 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS117Comparison_10-11.html)
* [CMS122 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS122Comparison_10-11.html)
* [CMS124 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS124Comparison_10-11.html)
* [CMS125 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS125Comparison_10-11.html)
* [CMS127 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS127Comparison_10-11.html)
* [CMS128 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS128Comparison_10-11.html)
* [CMS129 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS129Comparison_11-12.html)
* [CMS130 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS130Comparison_10-11.html)
* [CMS131 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS131Comparison_10-11.html)
* [CMS132 v7 vs v8](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS132Comparison_7-8.html)
* [CMS133 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS133Comparison_10-11.html)
* [CMS134 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS134Comparison_10-11.html)
* [CMS135 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS135Comparison_10-11.html)
* [CMS136 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS136Comparison_11-12.html)
* [CMS137 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS137Comparison_10-11.html)
* [CMS138 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS138Comparison_10-11.html)
* [CMS139 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS139Comparison_10-11.html)
* [CMS142 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS142Comparison_10-11.html)
* [CMS143 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS143Comparison_10-11.html)
* [CMS144 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS144Comparison_10-11.html)
* [CMS145 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS145Comparison_10-11.html)
* [CMS146 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS146Comparison_10-11.html)
* [CMS147 v11 vs v12](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS147Comparison_11-12.html)
* [CMS149 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS149Comparison_10-11.html)
* [CMS153 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS153Comparison_10-11.html)
* [CMS154 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS154Comparison_10-11.html)
* [CMS155 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS155Comparison_10-11.html)
* [CMS156 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS156Comparison_10-11.html)
* [CMS157 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS157Comparison_10-11.html)
* [CMS159 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS159Comparison_10-11.html)
* [CMS160 v7 vs v8](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS160Comparison_7-8.html)
* [CMS161 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS161Comparison_10-11.html)
* [CMS165 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS165Comparison_10-11.html)
* [CMS177 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS177Comparison_10-11.html)
* [CMS190 v10 vs v11](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS190Comparison_10-11.html)
* [CMS249 v4 vs v5](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS249Comparison_4-5.html)
* [CMS347 v5 vs v6](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS347Comparison_5-6.html)
* [CMS349 v4 vs v5](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS349Comparison_4-5.html)
* [CMS460 v1 vs v2](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS460Comparison_1-2.html)
* [CMS506 v4 vs v5](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS506Comparison_4-5.html)
* [CMS645 v5 vs v6](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS645Comparison_5-6.html)
* [CMS646 v2 vs v3](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS646Comparison_2-3.html)
* [CMS771 v3 vs v4](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS771Comparison_3-4.html)
* [CMS816 v1 vs v2](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS816Comparison_1-2.html)
* [CMS871 v1 vs v2](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS871Comparison_1-2.html)
* [CMS996 v2 vs v3](https://htmlpreview.github.io/?https://github.com/schallot/CQMParser/blob/master/CompiledMeasures/ComparisonReports/CMS996Comparison_2-3.html)