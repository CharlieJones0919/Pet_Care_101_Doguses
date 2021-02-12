using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using System.Linq;

public class DogGeneration : MonoBehaviour
{
    public enum DogBreed
    {
        French_Bulldog,
        German_Shepard,
        Labrador,
        Shih_Tzu,
        Beagle,
        Bulldog,
        Rottweiler,
        Welsh_Corgi,
        Dachshund,
        Yorkshire_Terrier,
        Siberian_Husky,
        Pomeranian,
        Scottish_Terrier,
        Pug,
        Chihuahua,
        Whippet,
        Basset_Hound,
        Springer_Spaniel,
        Icelandic_Sheepdog,
        Pekingese,
        Saint_Bernard,
        Xoloitzcuintli,
        Great_Dane,
        Irish_Wolfhound,
        Shiba_Inu
    }

    public enum DogDataField
    {
        Breed,
        Affection,
        Tolerance,
        Intelligence,
        Energy,
        Obedience,
        Coat_Kind,
        Grooming_Diff,
        Size,
        Body_Length,
        Leg_Length,
        Ear_Kind,
        Ear_Orientation,
        Snout_Kind,
        Snout_Length,
        Tail_Kind,
        Tail_Orientation
    }

    [SerializeField] private GameObject prefabBase;
    private string breedDataFileDir = "/Scripts/Dogs/ReferenceFiles/BreedData.txt";

    private DataTable breedData = new DataTable();
    private int numDataFields = 0;
    private int numDogBreedDefined = 0;

    private string GetBreedValue(DogBreed breed, DogDataField dataField)
    {
        for (int i = 0; i < numDogBreedDefined; i++)
        {
            if (breed.ToString() == breedData.Rows[i]["Breed"].ToString())
            {
                return breedData.Rows[i][dataField.ToString()].ToString();
            }
        }

        Debug.Log("Requested data for that breed cannot be found: " + breed.ToString() + " " + dataField.ToString());
        return null;
    }

    private void Start()
    {
        string[] data = System.IO.File.ReadAllLines(Application.dataPath + breedDataFileDir);
        for (int i = 0; i < data.Length; i++) //Remove Blank Spaces from Data
        {
            data[i] = string.Concat(data[i].Where(c => !char.IsWhiteSpace(c)));
        }

        numDataFields = DogDataField.GetNames(typeof(DogDataField)).Length;
        numDogBreedDefined = data.Length;

        breedData.Clear();

        string[] headers = new string[numDataFields];
        headers = data[0].Split('?');
        foreach (string column in headers) { breedData.Columns.Add(column); }

        for (int i = 1; i < numDogBreedDefined; i++)
        {
            string[] field = new string[numDataFields];
            field = data[i].Split('?');

            DataRow newData = breedData.NewRow();
            for (int j = 0; j < numDataFields; j++)
            {
                newData[breedData.Columns[j].ToString()] = field[j];
            }
            breedData.Rows.Add(newData);
        }

        Debug.Log(GetBreedValue(DogBreed.Dachshund, DogDataField.Coat_Kind));
    }

    public GameObject GenerateNewDog(DogBreed breed)
    {
        GameObject newDog = GenerateBreedPrefab(breed);
        return newDog;
    }

    private void InitialiseProperties(DogBreed breed)
    {

    }

    private GameObject GenerateBreedPrefab(DogBreed breed)
    {
        GameObject newDog = new GameObject("NewUnnamedDog");


        return null;
    }
}