﻿using PatientManager.Managers;
using PatientManager.Models;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PatientManager.Services
{
    public class PatientService
    {
        private readonly string filePath = "patients.txt";

        //CREATE LOGIC
        public PatientWithBlood CreatePatient(Patient patient)
        {

            if (patient == null || string.IsNullOrWhiteSpace(patient.Name) || string.IsNullOrWhiteSpace(patient.LastName) || string.IsNullOrWhiteSpace(patient.CI))
            {
                throw new ArgumentNullException("Datos del paciente invalidos");
            }

            var exists = GetAllPatients().FirstOrDefault(p => p.CI == patient.CI);
            if (exists != null)
            {
                throw new ArgumentNullException($"Ya existe un paciente con CI {patient.CI})");
            }


            string[] bloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            string randomBlood = bloodGroups[new Random().Next(bloodGroups.Length)];

            var fullPatient = new PatientWithBlood
            {
                Name = patient.Name,
                LastName = patient.LastName,
                CI = patient.CI,
                BloodGroup = randomBlood
            };

            string line = $"{fullPatient.Name},{fullPatient.LastName},{fullPatient.CI},{fullPatient.BloodGroup}";

            File.AppendAllLines(filePath, new[] { line });

            return fullPatient;
        }

        //GET ALL LOGIC 
        public List<PatientWithBlood> GetAllPatients()
        {
            var patients = new List<PatientWithBlood>();

            if (!File.Exists(filePath))
                return patients;

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var data = line.Split(',');

                if (data.Length == 4 && 
                    !string.IsNullOrWhiteSpace(data[0]) && 
                    !string.IsNullOrWhiteSpace(data[1]) && 
                    !string.IsNullOrWhiteSpace(data[2]) && 
                    !string.IsNullOrWhiteSpace(data[3]))
                {
                    patients.Add(new PatientWithBlood
                    {
                        Name = data[0].Trim(),
                        LastName = data[1].Trim(),
                        CI = data[2].Trim(),
                        BloodGroup = data[3].Trim()
                    });
                }
            }

            return patients;
        }

        //UPDATE LOGIC 
        public bool UpdatePatient(string ci, string newName, string newLastName)
        {
            if (string.IsNullOrWhiteSpace(ci)) 
            {
                return false; 
            }
            if (!File.Exists(filePath)) { return false; }

            var lines = File.ReadAllLines(filePath);    
            bool found = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var data = lines[i].Split(",");

                if (data.Length == 4 && data[2].Trim() == ci)
                {
                    lines[i] = $"{newName},{newLastName},{data[2].Trim()},{data[3].Trim()}";
                    found = true;
                    break;
                } 
            }

            if (found)
            {
                File.WriteAllLines(filePath, lines);
            }

            return found;
        }

        //DELETE LOGIC 

        public bool DeletePatient(string ci)
        {

            if (string.IsNullOrWhiteSpace(ci) || !File.Exists(filePath))
            {
                return false;
            }

            var lines = File.ReadAllLines(filePath);
            var updatedLines = new List<string>();
            bool found = false;

            foreach (var line in lines)
            {
                var data = line.Split(",");

                if (data.Length == 4 && data[2].Trim() == ci)
                {
                    found = true; //flag is true
                    continue; //omitimos la linea que queremos eliminar 
                }

                updatedLines.Add(line);
            }

            if (!found)
            {
                return false; //por si el ci no existe 
            }

            File.WriteAllLines(filePath, updatedLines);
            return true;
        }

        // GET BY CI LOGIC 

        public PatientWithBlood GetPatientByCi(string ci)
        {
            if (string.IsNullOrWhiteSpace(ci) || !File.Exists(filePath))
            {
                return null; 
            }

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var data = line.Split(",");

                if (data.Length == 4 && data[2].Trim() == ci)
                {
                    return new PatientWithBlood
                    {
                        Name = data[0].Trim(),
                        LastName = data[1].Trim(),
                        CI = data[2].Trim(),
                        BloodGroup = data[3].Trim(),
                    };
                }
            }

            return null; 
        }


        //LOGIC - GIFT DISTRIBUTION
        public Electronic AssingGiftToPatient(string ci)
        {
            if (string.IsNullOrWhiteSpace(ci) || !File.Exists(filePath))
            {
                return null;
            }

            var lines = File.ReadAllLines(filePath);
            Electronic assignedGift = new Electronic(); 

            foreach (var line in lines)
            {
                var data = line.Split(",");

                if (data.Length == 4 && data[2].Trim() == ci)
                {
                    GiftManager gm = new GiftManager();
                    List<Electronic> giftList = gm.GetGiftList();
                    
                    if (giftList.Count == 0)
                        return null;

                    Random rnd = new Random();
                    int index = rnd.Next(giftList.Count);
                    assignedGift = giftList[index];


                }
            }

            return assignedGift;
        }
    }
}
