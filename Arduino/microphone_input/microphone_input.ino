//Turns on the LED according to sound level recorded by the sound or microphone sensor.

int led1 = A1;

int led2 = A3;

int led3 = A4;


int sensorPin = A0; // input pin for the sensor

int sensorval = 0; // variable for the value coming from the sensor

int maxval = 0;
// the setup routine runs once when you press reset:
int count = 0;

void setup() { // initialize the digital pin as an output.

pinMode(led1,OUTPUT);

pinMode(led2,OUTPUT);

pinMode(led3,OUTPUT);


pinMode(sensorPin, INPUT); // initialize sensor as input

Serial.begin(9600); // initialize serial communication with computer
maxval = 0; 
}

void loop() {

  sensorval = analogRead(sensorPin); // read the value from the sensor , multiply by 60 for a sensitive calibration
  if (sensorval > maxval) {
     maxval = sensorval; 
  }  
  else { 
    count++;
    if (count == 10) {
      maxval = maxval-1;
      count = 0;
    }
  }
  Serial.println(maxval); // send it to the computer's serial port screen

  if (maxval > 800) { digitalWrite(led1, HIGH); } else { digitalWrite(led1,LOW); }

  if (maxval > 850) { digitalWrite(led2, HIGH); }else { digitalWrite(led2,LOW); }

  if (maxval > 900) { digitalWrite(led3, HIGH); } else { digitalWrite(led3,LOW); }

}