const int micPin = A0; 
<<<<<<< Updated upstream
int maxVal = 0; 
int count = 0;
=======
const int potPin = A3;
int micMaxVal = 0; 
int micCount = 0;
>>>>>>> Stashed changes

void setup() {
  Serial.begin(9600);
  pinMode(micPin, INPUT);
}

void loop() {
  int micValue = analogRead(micPin);
  if (micValue>maxVal){
    maxVal = micValue;
    Serial.println(maxVal);
  }
  else {
    count ++;
    if(count == 10) {
      maxVal = maxVal-1;
      count = 0;
    }
  }

<<<<<<< Updated upstream
=======
  int potValue = analogRead(potPin);
  Serial.print("Potentiometer: ");
  Serial.println(potValue);

>>>>>>> Stashed changes
}
