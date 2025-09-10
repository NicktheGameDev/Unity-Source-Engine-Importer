
$modelname "humans\guard.mdl"

$model "guard" "guard_base_male_combat.smd" {

	eyeball "eye_right" "ValveBiped.Bip01_Head1" -1.279000 -3.720001 67.250002 "eyeball_r" 1 4 "iris_unused" 0.63
	eyeball "eye_left" "ValveBiped.Bip01_Head1" 1.329000 -3.720001 67.250002 "eyeball_l" 1 -4 "iris_unused" 0.63

	eyelid upper_right "guard_01.vta" lowerer 1 -0.12 neutral 0 0.17 raiser 0 0.42 split 1 eyeball "eye_right"
	eyelid lower_right "guard_01.vta" lowerer 2 -0.25 neutral 0 -0.16 raiser 3 -0.07 split 1 eyeball "eye_right"
	eyelid upper_left "guard_01.vta" lowerer 4 -0.12 neutral 0 0.17 raiser 0 0.42 split -1 eyeball "eye_left"
	eyelid lower_left "guard_01.vta" lowerer 5 -0.25 neutral 0 -0.16 raiser 6 -0.07 split -1 eyeball "eye_left"

	mouth 0 "mouth" "ValveBiped.Bip01_Head1" 0 1 0

	flexfile "guard_01.vta" 
	{
		defaultflex frame 0
		// Already in eyelid lines: flex "upper_right" frame 1
		// Already in eyelid lines: flex "lower_right" frame 2
		// Already in eyelid lines: flex "lower_right" frame 3
		// Already in eyelid lines: flex "upper_left" frame 4
		// Already in eyelid lines: flex "lower_left" frame 5
		// Already in eyelid lines: flex "lower_left" frame 6
		flexpair "BU03" 1 frame 7
		flexpair "AU1" 1 frame 8
		flexpair "AU2" 1 frame 9
		flexpair "AU4" 1 frame 10
		flexpair "AU1AU2" 1 frame 11
		flexpair "AU12" 1 frame 12
		flex "AU12AU25" frame 13
		flexpair "AU15" 1 frame 14
		flexpair "AU17" 1 frame 15
		flexpair "AU10" 1 frame 16
		flex "AU16" frame 17
		flexpair "AU6" 1 frame 18
		flexpair "AU9" 1 frame 19
		flexpair "AU25" 1 frame 20
		flexpair "AU18" 1 frame 21
		flexpair "AU18" 1 frame 22
		flexpair "AU22" 1 frame 23
		flexpair "AU20" 1 frame 24
		flex "AU32" frame 25
		flex "AU24" frame 26
		flex "AU38" frame 27
		flex "AU31" frame 28
		flexpair "AU26" 1 frame 29
		flexpair "AU27" 1 frame 30
		flexpair "AU27Z" 1 frame 31
		flex "SH01" frame 32
		flex "SH02" frame 33
		flex "SH03" frame 34
		flex "SH04" frame 35
		flex "SH05" frame 36
		flex "SH06" frame 37
		flex "SH07" frame 38
		flex "SH08" frame 39
		flex "SH09" frame 40
		flex "SH10" frame 41
		flex "SH11" frame 42
		flex "SH12" frame 43
		flex "SH13" frame 44
		flex "SH14" frame 45
		flex "SH15" frame 46
		flex "SH16" frame 47
		flex "SH17" frame 48
		flex "SH18" frame 49
		flex "SH19" frame 50
		flex "SH20" frame 51
		flex "SH21" frame 52
		flex "SH22" frame 53
		flex "SH23" frame 54
		flex "SH24" frame 55
		flex "SH25" frame 56
		flex "SH26" frame 57
		flex "SH27" frame 58
		flex "SH28" frame 59
		flex "SH29" frame 60
		flex "SH30" frame 61
		flex "SH31" frame 62
		flex "SH32" frame 63
		flex "SH33" frame 64
		flex "SH34" frame 65
		flex "SH35" frame 66
		flex "SH36" frame 67
		flex "SH37" frame 68
		flex "SH38" frame 69
		flex "SH39" frame 70
		flex "SH40" frame 71
		flex "SH41" frame 72
		flex "SH42" frame 73
		flex "SH43" frame 74
		flex "SH44" frame 75
		flex "SH45" frame 76
		flex "SH46" frame 77
		flex "SH47" frame 78
		flex "SH48" frame 79
		flex "SH49" frame 80
		flex "SH50" frame 81
		flex "SH51" frame 82
		flex "SH52" frame 83
		flex "SH53" frame 84
		flex "SH54" frame 85
		flex "SH55" frame 86
		flex "SH56" frame 87
		flex "SH57" frame 88
		flex "SH58" frame 89
		flex "SH59" frame 90
		flex "SH60" frame 91
		flex "SH61" frame 92
		flex "SH62" frame 93
		flex "SH63" frame 94
		flex "SH64" frame 95
		flex "SH65" frame 96
		flex "SH66" frame 97
		flex "SH67" frame 98
		flex "SH68" frame 99
		flex "SH69" frame 100
		flex "SH70" frame 101
		flex "SH71" frame 102
		flex "SH72" frame 103
		flex "SH73" frame 104
		flex "SH74" frame 105
		flex "SH75" frame 106
		flex "SH76" frame 107
		flex "SH77" frame 108
		flex "SH79" frame 109
		flex "SH78" frame 110
	}

	flexcontroller eyelid range 0 1 "right_lid_raiser"
	flexcontroller eyelid range 0 1 "left_lid_raiser"
	flexcontroller eyelid range 0 1 "right_lid_tightener"
	flexcontroller eyelid range 0 1 "left_lid_tightener"
	flexcontroller eyelid range 0 1 "right_lid_droop"
	flexcontroller eyelid range 0 1 "left_lid_droop"
	flexcontroller eyelid range 0 1 "right_lid_closer"
	flexcontroller eyelid range 0 1 "left_lid_closer"
	flexcontroller eyelid range 0 1 "half_closed"
	flexcontroller eyelid range 0 1 "blink"
	flexcontroller brow range 0 1 "right_inner_raiser"
	flexcontroller brow range 0 1 "left_inner_raiser"
	flexcontroller brow range 0 1 "right_outer_raiser"
	flexcontroller brow range 0 1 "left_outer_raiser"
	flexcontroller brow range 0 1 "right_lowerer"
	flexcontroller brow range 0 1 "left_lowerer"
	flexcontroller nose range 0 1 "right_cheek_raiser"
	flexcontroller nose range 0 1 "left_cheek_raiser"
	flexcontroller nose range 0 1 "wrinkler"
	flexcontroller nose range 0 1 "dilator"
	flexcontroller mouth range 0 1 "right_upper_raiser"
	flexcontroller mouth range 0 1 "left_upper_raiser"
	flexcontroller mouth range 0 1 "right_corner_puller"
	flexcontroller mouth range 0 1 "left_corner_puller"
	flexcontroller mouth range 0 1 "right_corner_depressor"
	flexcontroller mouth range 0 1 "left_corner_depressor"
	flexcontroller mouth range 0 1 "chin_raiser"
	flexcontroller phoneme range 0 1 "right_part"
	flexcontroller phoneme range 0 1 "left_part"
	flexcontroller phoneme range 0 1 "right_puckerer"
	flexcontroller phoneme range 0 1 "left_puckerer"
	flexcontroller phoneme range 0 1 "right_funneler"
	flexcontroller phoneme range 0 1 "left_funneler"
	flexcontroller phoneme range 0 1 "right_stretcher"
	flexcontroller phoneme range 0 1 "left_stretcher"
	flexcontroller phoneme range 0 1 "bite"
	flexcontroller phoneme range 0 1 "presser"
	flexcontroller phoneme range 0 1 "tightener"
	flexcontroller phoneme range 0 1 "jaw_clencher"
	flexcontroller phoneme range 0 1 "jaw_drop"
	flexcontroller phoneme range 0 1 "right_mouth_drop"
	flexcontroller phoneme range 0 1 "left_mouth_drop"
	flexcontroller mouth range 0 1 "smile"
	flexcontroller mouth range 0 1 "lower_lip"
	flexcontroller shape range 0 1 "brow_h"
	flexcontroller shape range 0 1 "cheek_depth"
	flexcontroller shape range 0 1 "cheek_fat_max"
	flexcontroller shape range 0 1 "cheek_fat_min"
	flexcontroller shape range 0 1 "chin_butt"
	flexcontroller shape range 0 1 "chin_width"
	flexcontroller shape range 0 1 "chin_height"
	flexcontroller shape range 0 1 "face_d_min"
	flexcontroller shape range 0 1 "face_d_max"
	flexcontroller shape range 0 1 "head_height"
	flexcontroller shape range 0 1 "head_w_min"
	flexcontroller shape range 0 1 "head_w_max"
	flexcontroller shape range 0 1 "neck_size"
	flexcontroller shape range 0 1 "ears_angle"
	flexcontroller shape range 0 1 "ears_height"
	flexcontroller shape range 0 1 "eyes_ang_min"
	flexcontroller shape range 0 1 "eyes_ang_max"
	flexcontroller shape range 0 1 "eyes_height"
	flexcontroller shape range 0 1 "jaw_depth"
	flexcontroller shape range 0 1 "lowlip_size"
	flexcontroller shape range 0 1 "uplip_size"
	flexcontroller shape range 0 1 "mouth_w_min"
	flexcontroller shape range 0 1 "mouth_w_max"
	flexcontroller shape range 0 1 "mouth_h_min"
	flexcontroller shape range 0 1 "mouth_h_max"
	flexcontroller shape range 0 1 "mouth_depth"
	flexcontroller shape range 0 1 "nose_w_min"
	flexcontroller shape range 0 1 "nose_w_max"
	flexcontroller shape range 0 1 "nose_h_min"
	flexcontroller shape range 0 1 "nose_h_max"
	flexcontroller shape range 0 1 "nose_angle"
	flexcontroller shape range 0 1 "nose_d_max"
	flexcontroller shape range 0 1 "nost_height"
	flexcontroller shape range 0 1 "nost_width"
	flexcontroller shape range 0 1 "nose_tip"
	flexcontroller shape range 0 1 "cigar_mouth"
	flexcontroller shape range 0 1 "hairline_puff"
	flexcontroller head range -30 30 "head_rightleft"
	flexcontroller head range -15 15 "head_updown"
	flexcontroller head range -15 15 "head_tilt"
	flexcontroller eyes range -30 30 "eyes_updown"
	flexcontroller eyes range -30 30 "eyes_rightleft"
	flexcontroller body range -6 6 "move_rightleft"
	flexcontroller body range -10 6 "move_forwardback"
	flexcontroller body range -10 2 "move_updown"
	flexcontroller body range -30 30 "body_rightleft"
	flexcontroller chest range -30 30 "chest_rightleft"
	flexcontroller head range -0.2 0.2 "head_forwardback"
	flexcontroller gesture range -1 1 "gesture_updown"
	flexcontroller gesture range -1 1 "gesture_rightleft"

	localvar AU42a
	localvar AU42R
	localvar AU42L
	localvar right_open
	localvar left_open
	localvar right_lip_suppressor
	localvar left_lip_suppressor
	localvar right_depressor_suppressor
	localvar left_depressor_suppressor
	localvar right_corner_suppressor
	localvar left_corner_suppressor
	localvar right_drop_suppressor
	localvar left_drop_suppressor
	localvar right_drop
	localvar left_drop
	%upper_right_raiser = right_lid_raiser * (1 - right_lid_droop * 0.8) * (1 - right_lid_closer) * (1 - blink)
	%upper_left_raiser = left_lid_raiser * (1 - left_lid_droop * 0.8) * (1 - left_lid_closer) * (1 - blink)
	%upper_right_neutral = (1 - right_lid_droop * 0.8) * (1 - right_lid_raiser) * (1 - right_lid_closer) * (1 - blink)
	%upper_left_neutral = (1 - left_lid_droop * 0.8) * (1 - left_lid_raiser) * (1 - left_lid_closer) * (1 - blink)
	%upper_right_lowerer = right_lid_closer + blink * (1 - right_lid_closer)
	%upper_left_lowerer = left_lid_closer + blink * (1 - left_lid_closer)
	%lower_right_raiser = right_lid_closer + blink * 0.5 * (1 - right_lid_closer)
	%lower_left_raiser = left_lid_closer + blink * 0.5 * (1 - left_lid_closer)
	%lower_right_neutral = (1 - right_lid_closer) * (1 - 0.5 * blink) * (1 - right_lid_tightener * 0.5) * (1 - right_cheek_raiser * 0.25)
	%lower_left_neutral = (1 - left_lid_closer) * (1 - 0.5 * blink) * (1 - left_lid_tightener * 0.5) * (1 - left_cheek_raiser * 0.25)
	%lower_right_lowerer = 0
	%lower_left_lowerer = 0
	%BU03L = left_lid_tightener
	%BU03R = right_lid_tightener
	%AU1R = right_inner_raiser * (1 - right_outer_raiser) * (1 - right_lowerer)
	%AU1L = left_inner_raiser * (1 - left_outer_raiser) * (1 - left_lowerer)
	%AU2R = right_outer_raiser * (1 - right_inner_raiser) * (1 - right_lowerer)
	%AU2L = left_outer_raiser * (1 - left_inner_raiser) * (1 - left_lowerer)
	%AU1AU2R = right_inner_raiser * right_outer_raiser * (1 - right_lowerer)
	%AU1AU2L = left_inner_raiser * left_outer_raiser * (1 - left_lowerer)
	%AU4R = right_lowerer
	%AU4L = left_lowerer
	%AU6R = right_cheek_raiser
	%AU6L = left_cheek_raiser
	%AU9R = wrinkler
	%AU9L = wrinkler
	%AU38 = dilator
	%right_open = smile + right_part * (1 - smile) + right_puckerer + right_funneler + right_upper_raiser * 0.5 + lower_lip * 0.5
	%left_open = smile + left_part * (1 - smile) + left_puckerer + left_funneler + left_upper_raiser * 0.5 + lower_lip * 0.5
	%right_lip_suppressor = 1 / %right_open * (1 - presser) * (1 - bite)
	%left_lip_suppressor = 1 / %left_open * (1 - presser) * (1 - bite)
	%AU10R = right_upper_raiser * right_upper_raiser * 0.5 * %right_lip_suppressor
	%AU10L = left_upper_raiser * left_upper_raiser * 0.5 * %left_lip_suppressor
	%AU25R = right_part * right_part * %right_lip_suppressor * (1 - smile)
	%AU25L = left_part * left_part * %left_lip_suppressor * (1 - smile)
	%AU12AU25 = smile * smile * 0.5 * (%right_lip_suppressor + %left_lip_suppressor)
	%AU18R = right_puckerer * right_puckerer * %right_lip_suppressor
	%AU18L = left_puckerer * left_puckerer * %left_lip_suppressor
	%AU22R = right_funneler * right_funneler * %right_lip_suppressor
	%AU22L = left_funneler * left_funneler * %left_lip_suppressor
	%AU17R = chin_raiser * (1 - jaw_drop * 0.5) * (1 - bite) * (1 - 0.6 * (%AU25R + %AU18R + %AU22R)) * (1 - smile)
	%AU17L = chin_raiser * (1 - jaw_drop * 0.5) * (1 - bite) * (1 - 0.6 * (%AU25L + %AU18L + %AU22L)) * (1 - smile)
	%right_depressor_suppressor = 1 / (%AU18R * 0.8 + %AU22R * 0.8 + %AU17R + bite + right_corner_puller + right_stretcher + right_corner_depressor) * (1 - jaw_drop * 0.5) * (1 - smile * 0.5)
	%left_depressor_suppressor = 1 / (%AU18L * 0.8 + %AU22L * 0.8 + %AU17L + bite + left_corner_puller + left_stretcher + left_corner_depressor) * (1 - jaw_drop * 0.5) * (1 - smile * 0.5)
	%AU15R = right_corner_depressor * right_corner_depressor * %right_depressor_suppressor
	%AU15L = left_corner_depressor * left_corner_depressor * %left_depressor_suppressor
	%right_corner_suppressor = 1 / (%AU18R * 0.8 + %AU22R * 0.8 + %AU17R + bite + right_corner_puller + right_stretcher + right_corner_depressor) * (1 - jaw_drop * 0.5) * (1 - smile)
	%left_corner_suppressor = 1 / (%AU18L * 0.8 + %AU22L * 0.8 + %AU17L + bite + left_corner_puller + left_stretcher + left_corner_depressor) * (1 - jaw_drop * 0.5) * (1 - smile)
	%AU12R = right_corner_puller * right_corner_puller * %right_corner_suppressor
	%AU12L = left_corner_puller * left_corner_puller * %left_corner_suppressor
	%AU20R = right_stretcher * right_stretcher * %right_corner_suppressor
	%AU20L = left_stretcher * left_stretcher * %left_corner_suppressor
	%AU32 = bite
	%AU24 = presser + (1 - presser) * tightener
	%AU31 = jaw_clencher * jaw_clencher / (jaw_clencher + jaw_drop)
	%right_drop_suppressor = 1 / (0.5 * %right_open + right_mouth_drop) * (1 - presser) * (1 - bite)
	%left_drop_suppressor = 1 / (0.5 * %left_open + left_mouth_drop) * (1 - presser) * (1 - bite)
	%right_drop = right_mouth_drop * right_mouth_drop * %right_drop_suppressor
	%left_drop = left_mouth_drop * left_mouth_drop * %left_drop_suppressor
	%AU26R = jaw_drop * (1 - %right_drop) * jaw_drop / (jaw_clencher + jaw_drop)
	%AU26L = jaw_drop * (1 - %left_drop) * jaw_drop / (jaw_clencher + jaw_drop)
	%AU27R = jaw_drop * %right_drop * jaw_drop / (jaw_clencher + jaw_drop)
	%AU27L = jaw_drop * %left_drop * jaw_drop / (jaw_clencher + jaw_drop)
	%AU42a = blink
	%AU42L = left_lid_droop
	%AU42R = right_lid_droop
	%AU16 = lower_lip * lower_lip * 0.25 * (%right_lip_suppressor + %left_lip_suppressor) * (1 - 0.5 * (%AU27R + %AU27L))
	%mouth = %AU27R * 0.5 + %AU27L * 0.5 + %AU22R * 0.35 + %AU22L * 0.35 + %AU18R * 0.25 + %AU18L * 0.25 + %AU25R * 0.35 + %AU25L * 0.35 + %AU32 * 0.5 + %AU12AU25 + %AU16 * 0.4
	%SH03 = brow_h
	%SH06 = cheek_depth
	%SH09 = cheek_fat_max
	%SH10 = cheek_fat_min
	%SH11 = chin_butt
	%SH13 = chin_width
	%SH15 = chin_height
	%SH23 = face_d_min
	%SH24 = face_d_max
	%SH28 = head_height
	%SH29 = head_w_min
	%SH30 = head_w_max
	%SH36 = neck_size
	%SH38 = ears_angle
	%SH42 = ears_height
	%SH45 = eyes_ang_min
	%SH46 = eyes_ang_max
	%SH48 = eyes_height
	%SH52 = jaw_depth
	%SH56 = lowlip_size
	%SH58 = uplip_size
	%SH59 = mouth_w_min
	%SH60 = mouth_w_max
	%SH61 = mouth_h_min
	%SH62 = mouth_h_max
	%SH64 = mouth_depth
	%SH65 = nose_w_min
	%SH66 = nose_w_max
	%SH67 = nose_h_min
	%SH68 = nose_h_max
	%SH70 = nose_angle
	%SH72 = nose_d_max
	%SH74 = nost_height
	%SH76 = nost_width
	%SH77 = nose_tip
	%SH78 = hairline_puff
	%SH79 = cigar_mouth
}
$bodygroup "body"
{
	studio "guard_body.smd"
}
$bodygroup "helmet"
{
	studio "guard_helmet.smd"
	blank
}
$bodygroup "chest"
{
	studio "guard_tie.smd"
	studio "guard_vest.smd"
	studio "guard_walkietalkie.smd"
	blank
}
$bodygroup "holster"
{
	blank
	studio "guard_holster.smd"
	studio "guard_holster_empty.smd"
}
$bodygroup "flashlight"
{
	blank
	studio "guard_flashlight.smd"
}

$lod 25
{
	replacemodel "guard_base_male_combat.smd" "guard_base_male_combat_lod1.smd"
	replacemodel "guard_body.smd" "guard_body_lod1.smd"
	replacemodel "guard_helmet.smd" "guard_helmet_lod1.smd"
	replacemodel "guard_tie.smd" "guard_tie_lod1.smd"
	replacemodel "guard_vest.smd" "guard_vest_lod1.smd"
	replacemodel "guard_walkietalkie.smd" "guard_walkietalkie_lod1.smd"
	replacemodel "guard_holster.smd" "guard_holster_lod1.smd"
	replacemodel "guard_holster_empty.smd" "guard_holster_empty_lod1.smd"
	replacemodel "guard_flashlight.smd" "guard_flashlight_lod1.smd"
	replacebone "ValveBiped.Anim_Attachment_RH" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Anim_Attachment_LH" "ValveBiped.Bip01_L_Hand"
	facial
}
$lod 35
{
	replacemodel "guard_base_male_combat.smd" "guard_base_male_combat_lod2.smd"
	replacemodel "guard_body.smd" "guard_body_lod2.smd"
	replacemodel "guard_helmet.smd" "guard_helmet_lod2.smd"
	replacemodel "guard_tie.smd" "guard_tie_lod2.smd"
	replacemodel "guard_vest.smd" "guard_vest_lod2.smd"
	replacemodel "guard_walkietalkie.smd" "guard_walkietalkie_lod2.smd"
	replacemodel "guard_holster.smd" "guard_holster_lod2.smd"
	replacemodel "guard_holster_empty.smd" "guard_holster_empty_lod2.smd"
	replacemodel "guard_flashlight.smd" "guard_flashlight_lod2.smd"
	replacebone "ValveBiped.Anim_Attachment_RH" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Anim_Attachment_LH" "ValveBiped.Bip01_L_Hand"
	facial
}
$lod 45
{
	replacemodel "guard_base_male_combat.smd" "guard_base_male_combat_lod3.smd"
	replacemodel "guard_body.smd" "guard_body_lod3.smd"
	replacemodel "guard_helmet.smd" "guard_helmet_lod3.smd"
	replacemodel "guard_tie.smd" "guard_tie_lod3.smd"
	replacemodel "guard_vest.smd" "guard_vest_lod3.smd"
	replacemodel "guard_walkietalkie.smd" "guard_walkietalkie_lod3.smd"
	replacemodel "guard_holster.smd" "guard_holster_lod3.smd"
	replacemodel "guard_holster_empty.smd" "guard_holster_empty_lod3.smd"
	replacemodel "guard_flashlight.smd" "guard_flashlight_lod3.smd"
	replacebone "ValveBiped.Bip01_R_Hand" "ValveBiped.Bip01_R_Forearm"
	replacebone "ValveBiped.Bip01_R_Finger2" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger21" "ValveBiped.Bip01_R_Finger2"
	replacebone "ValveBiped.Bip01_R_Finger22" "ValveBiped.Bip01_R_Finger21"
	replacebone "ValveBiped.Bip01_R_Finger1" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger11" "ValveBiped.Bip01_R_Finger1"
	replacebone "ValveBiped.Bip01_R_Finger12" "ValveBiped.Bip01_R_Finger11"
	replacebone "ValveBiped.Bip01_R_Finger0" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger01" "ValveBiped.Bip01_R_Finger0"
	replacebone "ValveBiped.Bip01_R_Finger02" "ValveBiped.Bip01_R_Finger01"
	replacebone "ValveBiped.Bip01_R_Finger4" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger41" "ValveBiped.Bip01_R_Finger4"
	replacebone "ValveBiped.Bip01_R_Finger42" "ValveBiped.Bip01_R_Finger41"
	replacebone "ValveBiped.Bip01_R_Finger3" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger31" "ValveBiped.Bip01_R_Finger3"
	replacebone "ValveBiped.Bip01_R_Finger32" "ValveBiped.Bip01_R_Finger31"
	replacebone "ValveBiped.Bip01_L_Wrist" "ValveBiped.Bip01_L_Forearm"
	replacebone "ValveBiped.Bip01_L_Hand" "ValveBiped.Bip01_L_Forearm"
	replacebone "ValveBiped.Bip01_L_Finger2" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger21" "ValveBiped.Bip01_L_Finger2"
	replacebone "ValveBiped.Bip01_L_Finger22" "ValveBiped.Bip01_L_Finger21"
	replacebone "ValveBiped.Bip01_L_Finger1" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger11" "ValveBiped.Bip01_L_Finger1"
	replacebone "ValveBiped.Bip01_L_Finger12" "ValveBiped.Bip01_L_Finger11"
	replacebone "ValveBiped.Bip01_L_Finger0" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger01" "ValveBiped.Bip01_L_Finger0"
	replacebone "ValveBiped.Bip01_L_Finger02" "ValveBiped.Bip01_L_Finger01"
	replacebone "ValveBiped.Bip01_L_Finger3" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger31" "ValveBiped.Bip01_L_Finger3"
	replacebone "ValveBiped.Bip01_L_Finger32" "ValveBiped.Bip01_L_Finger31"
	replacebone "ValveBiped.Bip01_L_Finger4" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger41" "ValveBiped.Bip01_L_Finger4"
	replacebone "ValveBiped.Bip01_L_Finger42" "ValveBiped.Bip01_L_Finger41"
	replacebone "ValveBiped.Bip01_Neck1" "ValveBiped.Bip01_Spine4"
	replacebone "ValveBiped.Bip01_Head1" "ValveBiped.Bip01_Neck1"
	replacebone "ValveBiped.Bip01_R_Wrist" "ValveBiped.Bip01_R_Forearm"
	replacebone "ValveBiped.Bip01_R_Foot" "ValveBiped.Bip01_R_Calf"
	replacebone "ValveBiped.Bip01_L_Foot" "ValveBiped.Bip01_L_Calf"
	replacebone "ValveBiped.forward" "ValveBiped.Bip01_Head1"
	replacebone "ValveBiped.Anim_Attachment_RH" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Anim_Attachment_LH" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_R_Toe0" "ValveBiped.Bip01_R_Foot"
	replacebone "ValveBiped.Bip01_L_Toe0" "ValveBiped.Bip01_L_Foot"
	nofacial
}
$shadowlod
{
	replacemodel "guard_base_male_combat.smd" "guard_base_male_combat_lod4.smd"
	replacemodel "guard_body.smd" "guard_body_lod4.smd"
	replacemodel "guard_helmet.smd" "guard_helmet_lod4.smd"
	replacemodel "guard_tie.smd" "guard_tie_lod4.smd"
	replacemodel "guard_vest.smd" "guard_vest_lod4.smd"
	replacemodel "guard_walkietalkie.smd" "guard_walkietalkie_lod4.smd"
	replacemodel "guard_holster.smd" "guard_holster_lod4.smd"
	replacemodel "guard_holster_empty.smd" "guard_holster_empty_lod4.smd"
	replacemodel "guard_flashlight.smd" "guard_flashlight_lod4.smd"
	replacebone "ValveBiped.Bip01_R_Hand" "ValveBiped.Bip01_R_Forearm"
	replacebone "ValveBiped.Bip01_R_Finger2" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger21" "ValveBiped.Bip01_R_Finger2"
	replacebone "ValveBiped.Bip01_R_Finger22" "ValveBiped.Bip01_R_Finger21"
	replacebone "ValveBiped.Bip01_R_Finger1" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger11" "ValveBiped.Bip01_R_Finger1"
	replacebone "ValveBiped.Bip01_R_Finger12" "ValveBiped.Bip01_R_Finger11"
	replacebone "ValveBiped.Bip01_R_Finger0" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger01" "ValveBiped.Bip01_R_Finger0"
	replacebone "ValveBiped.Bip01_R_Finger02" "ValveBiped.Bip01_R_Finger01"
	replacebone "ValveBiped.Bip01_R_Finger4" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger41" "ValveBiped.Bip01_R_Finger4"
	replacebone "ValveBiped.Bip01_R_Finger42" "ValveBiped.Bip01_R_Finger41"
	replacebone "ValveBiped.Bip01_R_Finger3" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Bip01_R_Finger31" "ValveBiped.Bip01_R_Finger3"
	replacebone "ValveBiped.Bip01_R_Finger32" "ValveBiped.Bip01_R_Finger31"
	replacebone "ValveBiped.Bip01_L_Wrist" "ValveBiped.Bip01_L_Forearm"
	replacebone "ValveBiped.Bip01_L_Hand" "ValveBiped.Bip01_L_Forearm"
	replacebone "ValveBiped.Bip01_L_Finger2" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger21" "ValveBiped.Bip01_L_Finger2"
	replacebone "ValveBiped.Bip01_L_Finger22" "ValveBiped.Bip01_L_Finger21"
	replacebone "ValveBiped.Bip01_L_Finger1" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger11" "ValveBiped.Bip01_L_Finger1"
	replacebone "ValveBiped.Bip01_L_Finger12" "ValveBiped.Bip01_L_Finger11"
	replacebone "ValveBiped.Bip01_L_Finger0" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger01" "ValveBiped.Bip01_L_Finger0"
	replacebone "ValveBiped.Bip01_L_Finger02" "ValveBiped.Bip01_L_Finger01"
	replacebone "ValveBiped.Bip01_L_Finger3" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger31" "ValveBiped.Bip01_L_Finger3"
	replacebone "ValveBiped.Bip01_L_Finger32" "ValveBiped.Bip01_L_Finger31"
	replacebone "ValveBiped.Bip01_L_Finger4" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_L_Finger41" "ValveBiped.Bip01_L_Finger4"
	replacebone "ValveBiped.Bip01_L_Finger42" "ValveBiped.Bip01_L_Finger41"
	replacebone "ValveBiped.Bip01_Neck1" "ValveBiped.Bip01_Spine4"
	replacebone "ValveBiped.Bip01_Head1" "ValveBiped.Bip01_Neck1"
	replacebone "ValveBiped.Bip01_R_Wrist" "ValveBiped.Bip01_R_Forearm"
	replacebone "ValveBiped.Bip01_R_Foot" "ValveBiped.Bip01_R_Calf"
	replacebone "ValveBiped.Bip01_L_Foot" "ValveBiped.Bip01_L_Calf"
	replacebone "ValveBiped.forward" "ValveBiped.Bip01_Head1"
	replacebone "ValveBiped.Anim_Attachment_RH" "ValveBiped.Bip01_R_Hand"
	replacebone "ValveBiped.Anim_Attachment_LH" "ValveBiped.Bip01_L_Hand"
	replacebone "ValveBiped.Bip01_R_Toe0" "ValveBiped.Bip01_R_Foot"
	replacebone "ValveBiped.Bip01_L_Toe0" "ValveBiped.Bip01_L_Foot"
	nofacial
}

$surfaceprop "flesh"

$contents "solid"

$illumposition 0.081 -0.278 34.663

$eyeposition 0 0 70

$opaque

$cdmaterials "models\humans\guards\"
$cdmaterials "models\humans\scientists\"
$cdmaterials "models\Humans\"
$cdmaterials "models\Humans\Male\"
$cdmaterials "models\Humans\Base_Male\"

$texturegroup "skinfamilies"
{
	{ "Base_M_D"    }
	{ "Base_M_D2"   }
	{ "Base_M_03_D" }
	{ "Base_M_04_D" }
	{ "Base_M_05_D" }
	{ "Base_M_06_D" }
	{ "Base_M_07_D" }
	{ "Base_M_08_D" }
	{ "Base_M_09_D" }
	{ "Base_M_10_D" }
	{ "Base_M_11_D" }
	{ "Base_M_12_D" }
	{ "Base_M_13_D" }
	{ "Base_M_14_D" }
	{ "Base_M_15_D" }
}

// This list shows the VMT file names used in the SMD files.
// "Base_M_D.vmt"
// "eyeball_l.vmt"
// "eyeball_r.vmt"
// "base_mouth.vmt"
// "guard_body_d.vmt"
// "Base_M_D2.vmt"
// "Base_M_03_D.vmt"
// "Base_M_04_D.vmt"
// "Base_M_05_D.vmt"
// "Base_M_06_D.vmt"
// "Base_M_07_D.vmt"
// "Base_M_08_D.vmt"
// "Base_M_09_D.vmt"
// "Base_M_10_D.vmt"
// "Base_M_11_D.vmt"
// "Base_M_12_D.vmt"
// "Base_M_13_D.vmt"
// "Base_M_14_D.vmt"
// "Base_M_15_D.vmt"
// "guard_accessories_d.vmt"
// "guard_vest_d.vmt"

$attachment "eyes" "ValveBiped.Bip01_Head1" 3.2 -3.64 -0.02 rotate 0 -80 -90
$attachment "mouth" "ValveBiped.Bip01_Head1" 0.8 -5.8 -0.15 rotate 0 -80 -90
$attachment "anim_attachment_RH" "ValveBiped.Anim_Attachment_RH" 0 0 0 rotate -90 -90 0
$attachment "anim_attachment_LH" "ValveBiped.Anim_Attachment_LH" 0 0 0 rotate -90 -90 0
$attachment "anim_attachment_head" "ValveBiped.Bip01_Head1" 0 0 0 rotate -90 -90 0
$attachment "forward" "ValveBiped.forward" 0 0 0 rotate 0 0 0
$attachment "neck" "ValveBiped.Bip01_Neck1" 3 -4 0 rotate 0 -90 -90

// Clipping box or view bounding box.
$cbox 0 0 0 0 0 0

// Bounding box or hull. Used for collision with a world object.
$bbox -13 -13 0 13 13 72

// Hitbox info. Used for damage-based collision.
$hboxset "default"
$hbox 0 "ValveBiped.Bip01_Pelvis" -8.12 -7.36 -7.08 8.53 8.43 5.44
$hbox 0 "ValveBiped.Bip01_Spine" -4.51 -3.73 -7.47 12.34 11.01 7.63
$hbox 0 "ValveBiped.Bip01_Spine1" -2.41 -4.88 -8.4 11.15 9.5 8.51
$hbox 0 "ValveBiped.Bip01_Spine4" -4.06 -1.42 -7.56 5.94 8.58 7.44
$hbox 0 "ValveBiped.Bip01_Neck1" -4.32 -5.72 -4.64 5.71 3.66 4.63
$hbox 1 "ValveBiped.Bip01_Head1" -3.68 -7.98 -4.63 9.29 3.19 4.46
$hbox 5 "ValveBiped.Bip01_R_Clavicle" 0.36 -5.85 -7.34 10.36 2.15 2.66
$hbox 5 "ValveBiped.Bip01_R_UpperArm" -2.45 -4.23 -3.5 15.58 2.77 3.5
$hbox 5 "ValveBiped.Bip01_R_Forearm" -3.1 -2.82 -1.81 11.9 2.18 3.19
$hbox 5 "ValveBiped.Bip01_R_Hand" -2.92 -1.98 -2.66 5.18 2.04 2.87
$hbox 4 "ValveBiped.Bip01_L_Clavicle" 0.05 -6.39 -4.39 9.05 2.61 5.61
$hbox 4 "ValveBiped.Bip01_L_UpperArm" -0.89 -3.84 -3 14.11 4.16 3
$hbox 4 "ValveBiped.Bip01_L_Forearm" -0.92 -2.53 -3.41 11.08 2.47 1.59
$hbox 4 "ValveBiped.Bip01_L_Hand" -2.26 -1.74 -2.87 5.19 1.99 2.68
$hbox 7 "ValveBiped.Bip01_R_Thigh" -3 -4 -4.5 17 6 4.5
$hbox 7 "ValveBiped.Bip01_R_Calf" -3.48 -3.5 -4.64 20.629999 3.5 2.4
$hbox 7 "ValveBiped.Bip01_R_Foot" -3.39 -4.31 -4.39 9.42 4.59 1.05
$hbox 6 "ValveBiped.Bip01_L_Thigh" -1 -5 -4 19 5 4
$hbox 6 "ValveBiped.Bip01_L_Calf" -3.32 -3.5 -2.76 20.629999 3.5 4.64
$hbox 6 "ValveBiped.Bip01_L_Foot" -4.37 -5.16 -1.69 9.42 4.62 4.38
$skipboneinbbox

$definebone "ValveBiped.Bip01" "" 0 -0.533615 38.566898 0 0 90.000207 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Pelvis" "" 0 -0.533615 38.566898 0 0 90.000207 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Spine" "ValveBiped.Bip01_Pelvis" 0 3.345131 -2.9819 4.944219 90.000207 90.000207 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Spine1" "ValveBiped.Bip01_Spine" 4.018333 0 0 0 -1.675443 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Spine2" "ValveBiped.Bip01_Spine1" 3.518559 0 0 0 5.74883 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Spine4" "ValveBiped.Bip01_Spine2" 8.942638 0 0 0 11.120881 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Clavicle" "ValveBiped.Bip01_Spine4" 2.033356 1.00077 -1.93761 73.712167 168.618615 -95.596291 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_UpperArm" "ValveBiped.Bip01_R_Clavicle" 6.028141 -0.000008 0 -0.462071 -33.623806 93.935284 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Forearm" "ValveBiped.Bip01_R_UpperArm" 11.6926 0 -0.000004 -0.000009 -3.459055 0.000001 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Hand" "ValveBiped.Bip01_R_Forearm" 11.481699 0 0 6.108067 2.532067 -89.664456 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger2" "ValveBiped.Bip01_R_Hand" 3.883657 -0.050331 -0.431043 -1.512833 -33.672447 -4.922 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger21" "ValveBiped.Bip01_R_Finger2" 1.719576 -0.000004 0 -0.456887 -20.995242 -0.000001 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger22" "ValveBiped.Bip01_R_Finger21" 1.20919 0 0 -0.255579 -12.397436 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger1" "ValveBiped.Bip01_R_Hand" 3.859827 -0.148941 -1.33128 2.209714 -30.48142 -20.306714 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger11" "ValveBiped.Bip01_R_Finger1" 1.719429 0 -0.000001 -0.378884 -20.796654 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger12" "ValveBiped.Bip01_R_Finger11" 1.09967 0 0 -0.241268 -13.998049 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger0" "ValveBiped.Bip01_R_Hand" 0.806017 -0.359833 -1.31833 38.595466 -45.49033 68.995573 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger01" "ValveBiped.Bip01_R_Finger0" 1.789782 -0.000001 0 -0.361019 13.095242 0.000007 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger02" "ValveBiped.Bip01_R_Finger01" 1.206995 0 0 -0.551071 20.792926 0.000005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger4" "ValveBiped.Bip01_R_Hand" 3.859669 -0.132565 1.19311 -8.006914 -52.447756 27.762214 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger41" "ValveBiped.Bip01_R_Finger4" 1.312557 0.000002 0 -0.683647 -16.11495 -0.000002 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger42" "ValveBiped.Bip01_R_Finger41" 0.729362 -0.000001 0 -0.351455 -16.599784 0.000002 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger3" "ValveBiped.Bip01_R_Hand" 3.942287 0.050335 0.431043 -4.311194 -47.905178 8.995322 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger31" "ValveBiped.Bip01_R_Finger3" 1.539101 0.000002 0 -0.335414 -22.616937 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger32" "ValveBiped.Bip01_R_Finger31" 1.196316 0.000001 0 -0.553656 -24.680783 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Clavicle" "ValveBiped.Bip01_Spine4" 2.033352 1.00077 1.93766 -73.711593 168.619189 91.480733 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_UpperArm" "ValveBiped.Bip01_L_Clavicle" 6.028139 0 0 2.738846 -33.527937 -90.504986 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Forearm" "ValveBiped.Bip01_L_UpperArm" 11.6926 -0.000001 0 -0.000003 -3.459056 0.000001 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Wrist" "ValveBiped.Bip01_L_Forearm" 11.481701 0 0 -0.000003 -0.000001 0.072882 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Hand" "ValveBiped.Bip01_L_Forearm" 11.481701 0 0 -6.108477 2.532071 90.136572 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger2" "ValveBiped.Bip01_L_Hand" 3.883688 -0.046772 0.431443 1.250771 -33.682299 4.921827 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger21" "ValveBiped.Bip01_L_Finger2" 1.71958 0.000004 0 0.316044 -20.997639 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger22" "ValveBiped.Bip01_L_Finger21" 1.209179 0 0 0.176691 -12.398804 0.000001 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger1" "ValveBiped.Bip01_L_Hand" 3.859858 -0.137951 1.33247 -2.449396 -30.465202 20.308432 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger11" "ValveBiped.Bip01_L_Finger1" 1.719433 0.000004 0 0.233424 -20.798877 -0.000002 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger12" "ValveBiped.Bip01_L_Finger11" 1.09967 0 0 0.14874 -13.999128 -0.000003 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger0" "ValveBiped.Bip01_L_Hand" 0.806042 -0.34893 1.32126 -38.931791 -45.224018 -69.147414 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger01" "ValveBiped.Bip01_L_Finger0" 1.789782 0.000002 0 0.423242 13.093403 0.000007 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger02" "ValveBiped.Bip01_L_Finger01" 1.207003 -0.000001 0 0.645544 20.790346 0.000005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger3" "ValveBiped.Bip01_L_Hand" 3.942331 0.046764 -0.431443 3.288341 -52.174391 -9.256076 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger31" "ValveBiped.Bip01_L_Finger3" 1.539097 0 0 0.255537 -14.197722 0.000002 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger32" "ValveBiped.Bip01_L_Finger31" 1.196323 0.000001 0 0.421755 -24.683139 0.000003 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger4" "ValveBiped.Bip01_L_Hand" 3.859701 -0.142414 -1.19198 8.882218 -50.096446 -27.404514 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger41" "ValveBiped.Bip01_L_Finger4" 1.312557 0.000002 -0.000001 0.553759 -25.1941 0.000005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger42" "ValveBiped.Bip01_L_Finger41" 0.729366 -0.000001 0 0.28481 -13.997297 0.000005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Neck1" "ValveBiped.Bip01_Spine4" 3.307274 -0.000001 0 0 22.945701 179.999855 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Head1" "ValveBiped.Bip01_Neck1" 3.593712 0 0 0 23.295719 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Wrist" "ValveBiped.Bip01_R_Forearm" 11.481699 0 0 -0.000009 0 0.008595 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Thigh" "ValveBiped.Bip01_Pelvis" -3.89045 0 0 2.970505 -90.299295 -90.000207 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Calf" "ValveBiped.Bip01_R_Thigh" 17.848198 0 0 0 1.958771 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Foot" "ValveBiped.Bip01_R_Calf" 16.525297 0 0 2.336464 -61.399876 2.212247 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Thigh" "ValveBiped.Bip01_Pelvis" 3.89045 0 0 2.970162 -89.700553 -90.000207 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Calf" "ValveBiped.Bip01_L_Thigh" 17.848202 0 0 0 1.959516 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Foot" "ValveBiped.Bip01_L_Calf" 16.525196 0 0 -2.336631 -61.40045 -1.599412 0 0 0 0 0 0
$definebone "ValveBiped.forward" "ValveBiped.Bip01_Head1" 2 -3.000001 0 0 -75.999986 -89.999982 0 0 0 0 0 0
$definebone "ValveBiped.Anim_Attachment_RH" "ValveBiped.Bip01_R_Hand" 2.421066 -1.710648 0 -0.000001 -89.999975 -89.999982 0 0 0 0 0 0
$definebone "ValveBiped.Anim_Attachment_LH" "ValveBiped.Bip01_L_Hand" 2.676086 -1.712444 -0.000001 0.000001 89.999982 90.000037 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Toe0" "ValveBiped.Bip01_R_Foot" 6.879449 0.000001 0 -0.16908 -33.482113 -4.660038 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Toe0" "ValveBiped.Bip01_L_Foot" 6.879439 0.000001 0.000001 -0.16908 -33.482106 -0.418202 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Tie" "ValveBiped.Bip01_Spine4" -1.699303 9.814461 0.056892 -0.19767 -17.718663 -0.025898 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_Tie2" "ValveBiped.Bip01_Tie" -11.288853 -0.713155 0.038624 0.13665 6.300821 0.014668 0 0 0 0 0 0

$bonemerge "ValveBiped.Bip01_Pelvis"
$bonemerge "ValveBiped.Bip01_Spine"
$bonemerge "ValveBiped.Bip01_Spine1"
$bonemerge "ValveBiped.Bip01_Spine2"
$bonemerge "ValveBiped.Bip01_Spine4"
$bonemerge "ValveBiped.Bip01_R_Clavicle"
$bonemerge "ValveBiped.Bip01_R_UpperArm"
$bonemerge "ValveBiped.Bip01_R_Forearm"
$bonemerge "ValveBiped.Bip01_R_Hand"

$jigglebone "ValveBiped.Bip01_Tie"
{
	has_base_spring
	{
		base_mass 0
		stiffness 600
		damping 60
		left_constraint 0 0
		left_friction 0
		up_constraint 0 0
		up_friction 0
		forward_constraint -2 2
		forward_friction 0
	}
}
$jigglebone "ValveBiped.Bip01_Tie2"
{
	has_base_spring
	{
		base_mass 0
		stiffness 400
		damping 40
		left_constraint 0 0
		left_friction 0
		up_constraint -1 1
		up_friction 0
		forward_constraint -5 5
		forward_friction 0
	}
}

$ikchain "rhand" "ValveBiped.Bip01_R_Hand" knee 0.707 0.707 0
$ikchain "lhand" "ValveBiped.Bip01_L_Hand" knee 0.707 0.707 0
$ikchain "rfoot" "ValveBiped.Bip01_R_Foot" knee 0.707 -0.707 0
$ikchain "lfoot" "ValveBiped.Bip01_L_Foot" knee 0.707 -0.707 0

$sequence "batonidle1" {
	"guard_anims\batonidle1.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
	ikrule "rhand" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 0
	ikrule "lhand" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 1
	ikrule "rfoot" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 2
	ikrule "lfoot" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 3
}

$sequence "batonidle2" {
	"guard_anims\batonidle2.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
	ikrule "rhand" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 0
	ikrule "lhand" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 1
	ikrule "rfoot" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 2
	ikrule "lfoot" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 0 0 target 3
}

$sequence "ragdoll" {
	"guard_anims\ragdoll.smd"
	activity "ACT_DIERAGDOLL" 1
	fadein 0.2
	fadeout 0.2
	fps 30
	ikrule "rhand" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 100 100 target 0
	ikrule "lhand" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 100 100 target 1
	ikrule "rfoot" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 100 100 target 2
	ikrule "lfoot" release contact 0 fakeorigin 0 0 0 fakerotate 0 0 0 floor 0 height 0 radius 0 range 0 0 100 100 target 3
}

$includemodel "humans/guard_anims.mdl"
$includemodel "police_animations.mdl"
$includemodel "humans/male_choreo.mdl"
$includemodel "humans/male_gestures.mdl"
$includemodel "humans/male_postures.mdl"
$includemodel "humans/male_ss.mdl"
$includemodel "barney_animations.mdl"
$includemodel "barney_gestures.mdl"
$includemodel "barney_postures.mdl"

$collisionjoints "guard_physics.smd"
{
	$mass 60
	$inertia 5
	$damping 0.01
	$rotdamping 1.5
	$rootbone "valvebiped.bip01_pelvis"


	$jointconstrain "ValveBiped.Bip01_L_Thigh" x limit -25 20 0.2
	$jointconstrain "ValveBiped.Bip01_L_Thigh" y limit -73 6 0.2
	$jointconstrain "ValveBiped.Bip01_L_Thigh" z limit -93 30 0.2

	$jointmassbias "ValveBiped.Bip01_L_Calf" 2
	$jointconstrain "ValveBiped.Bip01_L_Calf" x limit 0 0 0.2
	$jointconstrain "ValveBiped.Bip01_L_Calf" y limit 0 0 0.2
	$jointconstrain "ValveBiped.Bip01_L_Calf" z limit -8 126 0.2

	$jointconstrain "ValveBiped.Bip01_R_Thigh" x limit -25 39 0.2
	$jointconstrain "ValveBiped.Bip01_R_Thigh" y limit -8 75 0.2
	$jointconstrain "ValveBiped.Bip01_R_Thigh" z limit -97 32 0.2

	$jointmassbias "ValveBiped.Bip01_R_Calf" 2
	$jointconstrain "ValveBiped.Bip01_R_Calf" x limit 0 0 0.2
	$jointconstrain "ValveBiped.Bip01_R_Calf" y limit 0 0 0.2
	$jointconstrain "ValveBiped.Bip01_R_Calf" z limit -12 126 0.2

	$jointmassbias "ValveBiped.Bip01_R_Foot" 2
	$jointconstrain "ValveBiped.Bip01_R_Foot" x limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_R_Foot" y limit -21 6 0.2
	$jointconstrain "ValveBiped.Bip01_R_Foot" z limit -15 35 0.2

	$jointconstrain "ValveBiped.Bip01_Spine2" x limit -48 48 0.2
	$jointconstrain "ValveBiped.Bip01_Spine2" y limit -25 25 0.2
	$jointconstrain "ValveBiped.Bip01_Spine2" z limit -25 50 0.2

	$jointconstrain "ValveBiped.Bip01_L_Clavicle" x limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_L_Clavicle" y limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_L_Clavicle" z limit 0 0 0

	$jointconstrain "ValveBiped.Bip01_L_UpperArm" x limit -50 57 0.2
	$jointconstrain "ValveBiped.Bip01_L_UpperArm" y limit -102 19 0.2
	$jointconstrain "ValveBiped.Bip01_L_UpperArm" z limit -93 30 0.2

	$jointmassbias "ValveBiped.Bip01_L_Forearm" 2
	$jointconstrain "ValveBiped.Bip01_L_Forearm" x limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_L_Forearm" y limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_L_Forearm" z limit -120 4 0.2

	$jointconstrain "ValveBiped.Bip01_R_Clavicle" x limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_R_Clavicle" y limit 0 0 0
	$jointconstrain "ValveBiped.Bip01_R_Clavicle" z limit 0 0 0

	$jointconstrain "ValveBiped.Bip01_R_UpperArm" x limit -50 57 0.2
	$jointconstrain "ValveBiped.Bip01_R_UpperArm" y limit -19 102 0.2
	$jointconstrain "ValveBiped.Bip01_R_UpperArm" z limit -93 30 0.2

	$jointmassbias "ValveBiped.Bip01_R_Forearm" 2
	$jointconstrain "ValveBiped.Bip01_R_Forearm" x limit 0 0 0.2
	$jointconstrain "ValveBiped.Bip01_R_Forearm" y limit 0 0 0.2
	$jointconstrain "ValveBiped.Bip01_R_Forearm" z limit -120 4 0.2

	$jointmassbias "ValveBiped.Bip01_R_Hand" 4
	$jointconstrain "ValveBiped.Bip01_R_Hand" x limit -60 60 0.2
	$jointconstrain "ValveBiped.Bip01_R_Hand" y limit -30 30 0.2
	$jointconstrain "ValveBiped.Bip01_R_Hand" z limit -57 70 0.2

	$jointmassbias "ValveBiped.Bip01_L_Hand" 4
	$jointconstrain "ValveBiped.Bip01_L_Hand" x limit -60 60 0.2
	$jointconstrain "ValveBiped.Bip01_L_Hand" y limit -30 30 0.2
	$jointconstrain "ValveBiped.Bip01_L_Hand" z limit -57 70 0.2

	$jointmassbias "ValveBiped.Bip01_Head1" 2
	$jointconstrain "ValveBiped.Bip01_Head1" x limit -20 20 0.2
	$jointconstrain "ValveBiped.Bip01_Head1" y limit -25 25 0.2
	$jointconstrain "ValveBiped.Bip01_Head1" z limit -13 30 0.2

	$jointmassbias "ValveBiped.Bip01_L_Foot" 2
	$jointconstrain "ValveBiped.Bip01_L_Foot" x limit -1 -1 0.2
	$jointconstrain "ValveBiped.Bip01_L_Foot" y limit -19 19 0.2
	$jointconstrain "ValveBiped.Bip01_L_Foot" z limit -15 35 0.2
}

$collisiontext
{
	break 
	{
		"model" "models/gibs/HGIBS.mdl"
		"health" "5"
		"debris" "1"
		"odds" "20"
		"random_velocity" "-50 50"
		"placementbone" "ValveBiped.Bip01_Head1"
	}
	break 
	{
		"model" "models/gibs/humans/eye_gib.mdl"
		"health" "5"
		"debris" "1"
		"odds" "80"
		"random_velocity" "-50 50"
		"placementbone" "ValveBiped.Bip01_Head1"
	}
	break 
	{
		"model" "models/gibs/humans/brain_gib.mdl"
		"health" "5"
		"debris" "1"
		"odds" "80"
		"random_velocity" "-50 50"
		"placementbone" "ValveBiped.Bip01_Head1"
	}
	break 
	{
		"model" "models/gibs/humans/heart_gib.mdl"
		"health" "5"
		"debris" "1"
		"odds" "80"
		"random_velocity" "-50 50"
		"placementbone" "ValveBiped.Bip01_Spine2"
	}
	break 
	{
		"model" "models/gibs/humans/lung_gib.mdl"
		"health" "5"
		"debris" "1"
		"odds" "80"
		"random_velocity" "-50 50"
		"placementbone" "ValveBiped.Bip01_Spine2"
	}
	break 
	{
		"model" "models/gibs/humans/liver_gib.mdl"
		"health" "5"
		"debris" "1"
		"odds" "80"
		"random_velocity" "-50 50"
		"placementbone" "ValveBiped.Bip01_Spine2"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_01.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_02.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_03.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_04.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_05.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_06.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"model" "models/gibs/humans/mgib_07.mdl"
		"health" "10"
		"debris" "1"
		"odds" "42"
		"random_velocity" "-50 50"
	}
	break 
	{
		"ragdoll" "models/gibs/humans/guard/torso.mdl"
		"health" "60"
		"particle" "gib_human_spurt"
		"debris" "1"
		"burst" "7"
	}
	break 
	{
		"ragdoll" "models/gibs/humans/guard/left_arm.mdl"
		"health" "35"
		"particle" "gib_human_spurt"
		"debris" "1"
		"burst" "7"
	}
	break 
	{
		"ragdoll" "models/gibs/humans/guard/right_leg.mdl"
		"health" "35"
		"particle" "gib_human_spurt"
		"debris" "1"
		"burst" "7"
	}
	break 
	{
		"ragdoll" "models/gibs/humans/guard/right_arm.mdl"
		"health" "35"
		"particle" "gib_human_spurt"
		"debris" "1"
		"burst" "7"
	}
}
