clc

%Get Data Files%
data = ('DogData.xlsx');
testData = xlsread(data);

%---------------------------------------------------------------------------------------------------------------------%


                                             % DOG OBEDIENCE FIS %
                                               
                                             
%---------------------------------------------------------------------------------------------------------------------%

DO = newfis('Dog Obedience');

%---------------------------------------------------------------%

                             %INPUT%

%---------------------------------------------------------------%

%AFFECTION VARIABLE%
DO = addvar(DO, 'input', 'Affection (0-5)', [0 5]); 
%AFFECTION MEMBERS%
DO = addmf(DO, 'input', 1, 'Aggressive', 'trapmf', [0 0 1.5 2.25]);
DO = addmf(DO, 'input', 1, 'Grouchy', 'trimf', [1.75 2.25 2.75]);
DO = addmf(DO, 'input', 1, 'Apathetic', 'trimf', [2.25 2.75 3.25]);
DO = addmf(DO, 'input', 1, 'Friendly', 'trimf', [2.75 3.25 4]);
DO = addmf(DO, 'input', 1, 'Loving', 'trapmf', [3.5 4 5 5]);

%TOLERANCE VARIABLE%
DO = addvar(DO, 'input', 'Tolerance (0-5)', [0 5]); 
%TOLERANCE MEMBERS%
DO = addmf(DO, 'input', 2, 'Nervous', 'gauss2mf', [0.5 0 0.5 1.25]);
DO = addmf(DO, 'input', 2, 'Neutral', 'gaussmf', [0.2 2.7]);
DO = addmf(DO, 'input', 2, 'Calm', 'smf', [2.75 5]);

%INTELLIGENCE VARIABLE%
DO = addvar(DO, 'input', 'Intelligence (0-5)', [0 5]); 
%INTELLIGENCE MEMBERS%
DO = addmf(DO, 'input', 3, 'Dumb', 'gaussmf', [1.25 0]);
DO = addmf(DO, 'input', 3, 'Average', 'gaussmf', [0.3 2.75]);
DO = addmf(DO, 'input', 3, 'Smart', 'gaussmf', [0.7 5]);

%ENERGY VARIABLE%
DO = addvar(DO, 'input', 'Energy (0-5)', [0 5]); 
%ENERGY MEMBERS%
DO = addmf(DO, 'input', 4, 'Sleepy', 'trimf', [0 0 2.5]);
DO = addmf(DO, 'input', 4, 'Normal', 'trapmf', [2.0 3 4 4.5]);
DO = addmf(DO, 'input', 4, 'Hyper', 'trapmf', [4.25 4.75 5 5]);

%---------------------------------------------------------------%

                            %OUTPUT%

%---------------------------------------------------------------%

%OBEDIENCE VARIABLE%
DO = addvar(DO, 'output', 'Obedience (0-5)', [0 5]); 
%OBEDIENCE MEMBERS%
DO = addmf(DO, 'output', 1, 'Bad', 'gaussmf', [1.25 0]);
DO = addmf(DO, 'output', 1, 'Average', 'trimf', [2.5 3 3.5]);
DO = addmf(DO, 'output', 1, 'Good', 'gaussmf', [0.85 5]);

%---------------------------------------------------------------%

                            %RULES%

%---------------------------------------------------------------%

% ??? BAD TRAITS ??? %
o_rule1 = [ 1  1  1  3, 1 (1.00) 1]; %Definitively Bad%

o_rule2 = [ 1  1  1  3, 1 (1.00) 2]; %Weighing as Bad (Aggressive/Hyper)%
o_rule3 = [ 1  1  1  1, 1 (0.85) 2]; %Weighing as Bad (Aggressive/Exhausted)%

o_rule4 = [ 2  1  1  3, 1 (0.90) 2]; %Weighing as Bad (Grouchy/Hyper)%
o_rule5 = [ 2  1  1  1, 1 (0.85) 2]; %Weighing as Bad (Grouchy/Exhausted)%

o_rule6 = [ 3  2  2  2, 2 (1.00) 1]; %Definitively Neutral%


o_rule7 = [ 4  3  3  0, 3 (0.90) 2]; %Weighing as Good (Friendly)%
o_rule8 = [ 5  3  3  0, 3 (1.00) 2]; %Weighing as Good (Loving)%

o_rule9 = [ 5  3  3  2, 3 (1.00) 1]; %Definitively Good%
% ??? GOOD TRAITS ??? %

DORuleList = ...
[
    o_rule1; o_rule2; o_rule3; o_rule4; o_rule5; o_rule6; o_rule7; o_rule8; o_rule9
];
DO = addrule(DO,DORuleList);

DORule = showrule(DO)
DO.defuzzMethod = 'bisector';

%---------------------------------------------------------------%

                         %WRITE TO FILE%

%---------------------------------------------------------------%

fprintf('\n OBEDIENCE DATA \n');
for i=1 : size(testData,1)
    DOoutput = evalfis([testData(i,1), testData(i,2), testData(i,3), testData(i,4)], DO);
    fprintf('%d)In(1): %.2f, In(2)%.2f, In(3)%.2f, In(4)%.2f => Out: %.2f \n', i, testData(i,1), testData(i,2), testData(i,3), testData(i,4), DOoutput);
    xlswrite('DogData.xlsx', DOoutput, 1, sprintf('L%d',i+2));
end
fprintf('\n');

%---------------------------------------------------------------------------------------------------------------------%


                                             % DOG PROPERTIES FIS %
                                               
                                             
%---------------------------------------------------------------------------------------------------------------------%

DP = newfis('Dog Properties');

%---------------------------------------------------------------%

                             %INPUT%

%---------------------------------------------------------------%

%ATTENTION VARIABLE%
DP = addvar(DP, 'input', 'Attention (%)', [0 100]); 
%ATTENTION MEMBERS%
DP = addmf(DP,'input', 1, 'Lonely', 'zmf', [0 60]);
DP = addmf(DP,'input', 1, 'Loved', 'gauss2mf', [10 65 10 75]);
DP = addmf(DP,'input', 1, 'Crowded', 'smf', [75 100]);

%HUNGER VARIABLE%
DP = addvar(DP, 'input', 'Hunger (%)', [0 100]); 
%HUNGER MEMBERS%
DP = addmf(DP, 'input', 2, 'Starving', 'zmf', [0 25]);
DP = addmf(DP, 'input', 2, 'Fed', 'gauss2mf', [15 60 9 70]);
DP = addmf(DP, 'input', 2, 'Overfed', 'smf', [75 100]);

%REST VARIABLE%
DP = addvar(DP, 'input', 'Rest (%)', [0 100]); 
%REST MEMBERS%
DP = addmf(DP,'input', 3, 'Exhausted', 'zmf', [0 30]);
DP = addmf(DP,'input', 3, 'Tired', 'gaussmf', [7.5 35]);
DP = addmf(DP,'input', 3, 'Rested', 'smf', [40 100]);

%HYGIENE VARIABLE%
DP = addvar(DP, 'input', 'Hygiene (%)', [0 100]); 
%HYGIENE MEMBERS%
DP = addmf(DP,'input', 4, 'Filthy', 'zmf', [0 25]);
DP = addmf(DP,'input', 4, 'Dirty', 'gaussmf', [7.5 40]);
DP = addmf(DP,'input', 4, 'Clean', 'smf', [40 100]);

%---------------------------------------------------------------%

                            %OUTPUT%

%---------------------------------------------------------------%

%BOND ADJUSTMENT VARIABLE%
DP = addvar(DP, 'output', 'Bond Adjustment', [-2 2]); 
%BOND ADJUSTMENT MEMBERS%
DP = addmf(DP, 'output', 1, 'Wary', 'zmf', [-2 0]);
DP = addmf(DP, 'output', 1, 'Friendly', 'smf', [0 2]);

%HEALTH ADJUSTMENT VARIABLE%
DP = addvar(DP, 'output', 'Health Adjustment', [-2 2]); 
%HEALTH ADJUSTMENT MEMBERS%
DP = addmf(DP, 'output', 2, 'Dying', 'zmf', [-2 -1.5]);
DP = addmf(DP, 'output', 2, 'Sick', 'gaussmf', [0.3 -0.9]);
DP = addmf(DP, 'output', 2, 'Good', 'smf', [0 2]);

%HAPPINESS ADJUSTMENT VARIABLE%
DP = addvar(DP, 'output', 'Happiness Adjustment', [-2 2]); 
%HAPPINESS ADJUSTMENT MEMBERS%
DP = addmf(DP, 'output', 3, 'Distressed', 'zmf', [-2 -1.5]);
DP = addmf(DP, 'output', 3, 'Upset', 'gaussmf', [0.35 -0.9]);
DP = addmf(DP, 'output', 3, 'Happy', 'smf', [0 2]);

%---------------------------------------------------------------%

                            %RULES%

%---------------------------------------------------------------%

%Bond Rules%
p_rule1 = [ 1  1  0  1,  1  0  0 (1.00) 2]; 
p_rule2 = [ 3  0  0  2,  1  0  0 (0.50) 2]; 
p_rule3 = [ 2 -1  0  3,  2  0  0 (1.00) 2]; 

%Health Rules%
p_rule4 = [ 0  1  1  1,  0  1  0 (1.00) 2]; 
p_rule5 = [ 0  3  2  2,  0  2  0 (1.00) 2]; 
p_rule6 = [ 0  2  3  3,  0  3  0 (1.00) 2]; 

%Happiness Rules%
p_rule7 = [ 1  1  1  1,  1  0  1 (1.00) 2]; 
p_rule8 = [ 3  2  2  2,  0  0  2 (1.00) 2]; 
p_rule9 = [ 2  3  3  3,  0  0  3 (1.00) 2]; 

DPRuleList = ...
[
    p_rule1; p_rule2; p_rule3; p_rule4; p_rule5; p_rule6; p_rule7; p_rule8; p_rule9
];
DP = addrule(DP,DPRuleList);

DPRule = showrule(DP)

DP.defuzzMethod = 'bisector';

%---------------------------------------------------------------%

                         %WRITE TO FILE%

%---------------------------------------------------------------%

fprintf('\n PROPERTIES DATA \n');
for i=1 : size(testData,1)
    DPoutput = evalfis([testData(i,5), testData(i,6), testData(i,7), testData(i,8)], DP);
    fprintf('%d)In(1): %.2f, In(2)%.2f, In(3)%.2f, In(4)%.2f => Out(1): %.2f, Out(2): %.2f, Out(3): %.2f \n', i, testData(i,5), testData(i,6), testData(i,7), testData(i,8), DPoutput);
    xlswrite('DogData.xlsx', DPoutput, 1, sprintf('M%d', i+2));
end
fprintf('\n');

fprintf('/////// FINISHED ///////');