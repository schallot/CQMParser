# CQMParser

Electronic Clinical Quality Measures are defined in HTML docs provided at https://ecqi.healthit.gov.  For example,
[CMS72v10](https://ecqi.healthit.gov/sites/default/files/ecqm/measures/CMS72v10.html).
These documents primarially define how patient populations are calculated for the metric, which involves a fairly regular pseudocode involving nested definitions.

This tool is designed to take an eCQM document, and recursively find and replace as many of the concepts as possible so that the population criteria are spelled out as explicitly as possible.  For example, for CMS72v10, the numerator is expressed as 

	"Encounter with Antithrombotic Therapy"

This tool [expands this line](https://github.com/schallot/CQMParser/blob/master/CMS/CMS72v10.txt) out as

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
	
should generate an output similar to [CMS104v11.txt](https://github.com/schallot/CQMParser/blob/master/CMS/CMS104v11.txt).  The results will be written to the CMS directory.  If a file is already present for the measure, it will be overwritten.
