# MungLearning
<p align="center"><img src="https://github.com/user-attachments/assets/1a07117b-acb7-40d2-95e7-38638b039c28" width="50%" height="60%"></p>

## 프로젝트 소개
- 이 프로젝트는 유기동물의 입양 가능성을 분류해 높은 순서대로 선정하는 머신러닝 모델 프로젝트 
- 유기견의 다양한 특징을 반영하여 모델에 적용 후 사용자가 이용하기 쉽도록 GUI를 제공
- 2024 천안시 데이터분석 아이디어 경진대회 우수상(3등) 수상

## 목적
- 유기견 입양률을 높여 전체적인 안락사와 폐사 비율을 줄이고자 유기견과의 직접적인 교감을 통해 유기동물 입양 캠페인을 위한 입양 후보 선정 시스템 제공
- 입양 캠페인에 데려갈 유기견 선정하는 실무에 사용할 수 있도록 제작

## 기능
- 기간 설정 및 선정 개체수 입력
- [국가동물보호정보시스템 공공데이터](https://www.data.go.kr/data/15098931/openapi.do) 실시간 API 연결
- LLM기반 데이터 전처리
- 머신러닝 분류 모델 추론
- 유기번호 및 해당 개체 사진 출력

## 데이터 셋
- 출처 : [국가동물보호정보시스템](https://www.animal.go.kr/front/awtis/public/publicAllList.do?menuNo=1000000064) 
- 기간 : 2014.01.01 ~ 2024.08.20
- 지역 : 충청남도 천안시
- 2,580건
<img width="280" alt="중성화가 결과에 미치는 그래프" src="https://github.com/user-attachments/assets/c7d42597-619f-4919-8bdf-d2f22d572eae">
<img width="280" alt="무게가 결과에 미치는 그래프" src="https://github.com/user-attachments/assets/d79b6f15-34f4-4538-9c66-bd88f932fd1a">

# EDA
- 결측치제거
- 이상치제거
- 라벨데이터 설정
  
# 전처리
- LLM을 사용 : 건강, 성격, 색
- Encoding
    - 성격, 색, 성별, 성격, 중성화유무, 상태(입양 여부) : Label Encoding
    - 품종 : Frequency Encoding
    - 건강 : Target Encoding
    - 무게 : Standard Scaling
- 오버샘플링
    - SMOTE

## 모델 
### gpt-4o
### XGBoost Classifier
### [Checkpoint](https://drive.google.com/file/d/1i9Bn7NZmIdqoyfQhrex2E8OrEbZUsWEg/view?usp=drive_link)

## 학습결과
<img width="280" alt="결과" src="https://github.com/user-attachments/assets/6102b864-1524-417a-9ff8-88177e0bbc33">

- 정확도 : 72.78%
- 정밀도 : 63.28%
- 재현율 : 60.00%
- F1 : 61.60%
- AUC : 78.04%

## 실행파일
### [다운로드](https://drive.google.com/file/d/1zbmOsAMcYw6MjoSw-Kl3qzVRaxExrm5R/view?usp=sharing)
