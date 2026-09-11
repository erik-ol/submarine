const int micPin = A0; 
int maxVal = 0; 
int count = 0;

void setup() {
  Serial.begin(9600);
  pinMode(micPin, INPUT);
}

void loop() {
  int micValue = analogRead(micPin);
  if (micValue>maxVal){
    maxVal = micValue;
  }
  else {
    count ++;
    if(count == 10) {
      maxVal = maxVal-1;
      count = 0;
    }
  }
  Serial.println(maxVal);
}
