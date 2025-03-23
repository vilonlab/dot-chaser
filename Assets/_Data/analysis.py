import os, sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

def plot_gaze_trace(df):
    # virt_pos_x,virt_pos_y,virt_pos_z,phys_pos_x,phys_pos_y,phys_pos_z,virt_heading_x,virt_heading_y,virt_heading_z,phys_heading_x,phys_heading_y,phys_heading_z,cyclopean_gaze_pos_x,cyclopean_gaze_pos_y,cyclopean_gaze_pos_z,cyclopean_gaze_dir_x,cyclopean_gaze_dir_y,cyclopean_gaze_dir_z,left_gaze_dir_x,left_gaze_dir_y,left_gaze_dir_z,right_gaze_dir_x,right_gaze_dir_y,right_gaze_dir_z,cyclopean_gaze_angular_velocity,weighted_gaze_angular_velocity,left_eye_closed,right_eye_closed,gaze_state,timestamp,frame_number,unity_delta_time
    gaze_x = list(df['cyclopean_gaze_pos_x'])
    gaze_z = list(df['cyclopean_gaze_pos_z'])

    plt.plot(gaze_x, gaze_z)
    plt.show()

    exit()

def main(log_file):
    df = pd.read_csv(log_file)
    plot_gaze_trace(df)

if __name__ == "__main__":
    data_file = 'ptpnt0_log_data.csv'

    main(data_file)