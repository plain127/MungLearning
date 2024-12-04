import io
import json
import os
import pickle
import sys

import pandas as pd
import torch
import xgboost as xgb

from llm_model import LlmModel

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

class DogInference():
    _instance = None
    def __init__(self, model_path):
        self.model = xgb.Booster()
        self.model.load_model(model_path)
        self.input_df = None
        self.llmModel = LlmModel() 
        self.device = 'cuda' if torch.cuda.is_available() else 'cpu'
        
        self.character_labels = ['사람을 경계함', '사람을 잘 따름', '중립']
        self.color_labels = ['밝은계열', '어두운계열', '혼합색계열']
        self.sex_labels = ['수컷', '암컷']
        self.neutering_labels = ['아니오', '예', '미상']
        self.health_labels = ['건강', '질병']
        self.breed_labels = ['믹스견', '품종견']
        self.target_labels = ['미입양', '입양']
        
        
        # Load scalers and encoders
        with open(os.path.dirname(os.path.abspath(__file__))+'\\scaler\\minmax_scaler_weight_age.pkl', 'rb') as f:
            self.weight_scaler = pickle.load(f)
    
    @staticmethod
    def get_instance(model_path):
        if DogInference._instance is None:
            DogInference._instance = DogInference(model_path)
        return DogInference._instance
         
    def load_csv(self, file_path):
        # CSV 파일을 읽어서 데이터프레임으로 변환
        try:
            self.input_df = pd.read_csv(file_path)
        except FileNotFoundError:
            print(f"[ERROR] 파일을 찾을 수 없습니다: {file_path}")
            return
        except pd.errors.EmptyDataError:
            print(f"[ERROR] 빈 파일입니다: {file_path}")
            return
        except pd.errors.ParserError:
            print(f"[ERROR] 파일을 파싱할 수 없습니다: {file_path}")
            return
    
    def prepare_data(self):
        self.input_df.rename(columns={' 품종':'품종_freq', ' 색상':'색', ' 체중':'무게(Kg)', ' 성별':'성별', ' 중성화유무':'중성화유무'}, inplace=True)
        self.llmModel.split_etc(self.input_df)
        self.llmModel.health_preprocess(self.input_df)
        self.llmModel.character_preprocess(self.input_df)
        self.llmModel.color_preprocess(self.input_df)
        # self.llmModel.log_cache_info()
        print('EDA 완료') 
        
    def preprocessing(self):
        # Process each row of input_df
        for index, row in self.input_df.iterrows():
            weight = float(row['무게(Kg)'].split('(')[0])
            weight_scaled = self.weight_scaler.transform([[weight]])
            self.input_df.at[index, '무게(Kg)'] = weight_scaled[0][0]
            
            health = pd.DataFrame([row['건강']], columns=['건강'])
            health_targetencoded = self.health_encoder.transform(health)
            self.input_df.at[index, '건강'] = health_targetencoded.iloc[0, 0]
            
            birth_year = int(row[' 나이'][:4]) 
            receipt_year = int(str(row[' 접수일시'])[:4])
            self.input_df.at[index, '나이'] = (receipt_year - birth_year)*12 + int(str(row[' 접수일시'])[3:6])
            self.input_df.at[index, '색'] = self.color_labels.index(row['색'])
            self.input_df.at[index, '성별'] = self.sex_labels.index(row['성별'])
            self.input_df.at[index, '중성화유무'] = self.neutering_labels.index(row['중성화유무'])
            self.input_df.at[index, '성격'] = self.character_labels.index(row['성격'])
            
            breed = str(row['품종_freq'][4:])
            self.input_df.at[index, '품종_freq'] = self.breed_freq[breed]

        self.input_df.drop(columns=[' 나이', ' 접수일시'], axis=1, inplace=True)
        
    def classification(self,num):
        self.prepare_data()
        self.preprocessing()
        
        print(self.input_df.head())
        # Model prediction
        df = self.input_df[['성별', '무게(Kg)', '나이', '성격', '색', '중성화유무', '건강', '품종_freq']]
        
        df['성별'] = df['성별'].astype('int32')
        df['성격'] = df['성격'].astype('int32')
        df['색'] = df['색'].astype('int32')
        df['중성화유무'] = df['중성화유무'].astype('int32')
        df['나이'] = df['나이'].astype('int64')
        df['무게(Kg)'] = df['무게(Kg)'].astype('float64')
        df['건강'] = df['건강'].astype('float64')
        df['품종_freq'] = df['품종_freq'].astype('float64')
        
        dmat = xgb.DMatrix(df)
        result = self.model.predict(dmat)

        self.input_df['상태'] = result
        self.input_df = self.input_df.sort_values(by='상태', ascending=False)
    
        dog_num = list(self.input_df['유기번호'][:num].values)
        img_urls = list(self.input_df[' 사진'][:num].values)
        
        return dog_num, img_urls


script_dir = os.path.dirname(os.path.abspath(__file__))
model_path = script_dir + '\\model_state\\best_model.xgb'

dog_inference = DogInference.get_instance(model_path)
