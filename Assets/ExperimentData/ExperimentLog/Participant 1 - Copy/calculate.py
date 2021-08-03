import itertools
import csv
import os

# Get participant number 
cwd = os.getcwd()
if cwd[-2] == " ":
  pNum = cwd[-1]
else:
  pNum = cwd[-2] + cwd[-1]

# Retrieve Manhattan Distance Error
counter = 0
counter2 = 0
r = 12
result = []
trialList = [[0 for x in range(5)] for y in range(18)]
answerList = [[0 for x in range(5)] for y in range(18)]

with open('trialCards.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    line_count = 0
    for row in csv_reader:
      for i in range(5):
        trialList[line_count][i] = row[i]
      line_count += 1
    
with open('answerCards.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    line_count = 0
    for row in csv_reader:
      for i in range(5):
        answerList[line_count][i] = row[i]
      line_count += 1

for aList in list(trialList):
  bList = answerList[trialList.index(aList)]
  matrix = [[0 for x in range(5)] for y in range(5)]
  for tmpA in list(aList):
    a = int(tmpA)
    for tmpB in list(bList):
      b = int(tmpB)
      if counter2 % 2 == 0:
          if int(pNum) % 2 == 0:
              if abs(a % r - b % r) == 11:
                  matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a / r) - int(b / r)) + 1)
              else:
                  matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a / r) - int(b / r)) + abs(a % r - b % r))
          else:
              matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a / r) - int(b / r)) + abs(a % r - b % r))
      else:
          if int(pNum) % 2 == 0:
              matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a / r) - int(b / r)) + abs(a % r - b % r))
          else:
              if abs(a % r - b % r) == 11:
                  matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a / r) - int(b / r)) + 1)
              else:
                  matrix[bList.index(tmpB)][aList.index(tmpA)] = (abs(int(a / r) - int(b / r)) + abs(a % r - b % r))
  result.append(min([sum([matrix[p[0]][p[1]] for p in enumerate(perm)]) for perm in itertools.permutations(range(len(matrix)))]))
  counter2 += 1

resultString = "Layout,Error\n"
for a in list(result):
    if counter % 2 == 0:
        if int(pNum) % 2 == 0:
          resultString += "Full Circle," + str(a) + "\n"
        else:
          resultString += "Flat," + str(a) + "\n"
    else:
        if int(pNum) % 2 == 0:
            resultString += "Flat," + str(a) + "\n"
        else:
            resultString += "Full Circle," + str(a) + "\n"
    counter += 1

f = open("result_" + pNum + "_ManhattanAccuracy.csv", "w")
f.write(resultString)
f.close()

# Retrieve answer accuracy
resultString = "Layout,Accuracy\n"
with open('Participant_' + pNum + '_Answers.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    next(csv_reader)
    for row in csv_reader:
        resultString += row[3] + "," + str(float(row[5]) / float(row[4])) + "\n"

f = open("result_" + pNum + "_Accuracy.csv", "w")
f.write(resultString)
f.close()

# Retrieve binary accuracy
resultString = "Layout,Accuracy\n"
with open('Participant_' + pNum + '_Answers.csv') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    next(csv_reader)
    for row in csv_reader:
        if float(row[5]) / float(row[4]) == 1:
            resultString += row[3] + ",1\n"
        else:
            resultString += row[3] + ",0\n"

f = open("result_" + pNum + "_BinaryAccuracy.csv", "w")
f.write(resultString)
f.close()