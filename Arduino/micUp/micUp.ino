const int micPin = A0; 
const int potPin = A3;
int micMaxVal = 0; 
int micCount = 0;
int potLastVal = 0; 

void setup() {
  Serial.begin(9600);
  pinMode(micPin, INPUT);
  pinMode(potPin, INPUT);
}

void loop() {
  int micValue = analogRead(micPin);
  if (micValue>micMaxVal){
    micMaxVal = micValue;
    Serial.print("Microphone: ");
    Serial.println(micMaxVal);
  }
  else {
    micCount ++;
    if(micCount == 10) {
      micMaxVal = micMaxVal-1;
      micCount = 0;
    }
  }

  int potValue = analogRead(potPin);
  if (potValue != potLastVal)
  {
    Serial.print("Potentiometer: ");
    Serial.println(potValue);
    potLastVal = potValue;
  }

}
